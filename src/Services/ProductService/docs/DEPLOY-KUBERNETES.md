# Deploy ProductService on Local Kubernetes and AWS EKS

This guide covers deploying **ProductService** to:

1. **Local Kubernetes** (Docker Desktop, minikube, kind)
2. **AWS EKS**

ProductService expects **PostgreSQL** in the same cluster (namespace `amcart-databases`, service `postgres:5432`). Deploy the database first—see [Databases – Deploy](../../Databases/README.md).

**Migrations:** We use **EF Core code-first with auto-migrate on startup**. When the service starts, it applies any pending migrations to the database. You do **not** need to run `dotnet ef database update` manually before deploying.

**JWT:** ProductService validates JWTs issued by UserService. Both services share the same JWT secret, issuer, and audience. Ensure the values in `secret.yaml` / `configmap.yaml` match UserService.

---

## Part 1: Local Kubernetes (Docker Desktop / minikube / kind)

### 1.1 Prerequisites

- Kubernetes enabled (e.g. Docker Desktop → Settings → Kubernetes → Enable).
- PostgreSQL deployed in the cluster with the `amcart_products` database:
  ```bash
  kubectl apply -k src/Databases/postgres/kubernetes/overlays/local
  ```
- **UserService** deployed (required for JWT token issuance):
  ```bash
  kubectl apply -k src/Services/UserService/kubernetes/overlays/local
  ```

### 1.2 Build the Docker image

The Dockerfile expects build context to include a `Services/` directory. From the **repository root**:

```bash
cd src
docker build -f Services/ProductService/Dockerfile -t product-service:latest .
```

For **minikube** (use the Docker daemon inside minikube so the image is available):

```bash
eval $(minikube docker-env)
cd src
docker build -f Services/ProductService/Dockerfile -t product-service:latest .
```

### 1.3 Deploy ProductService

From the **repository root**:

```bash
kubectl apply -k src/Services/ProductService/kubernetes/overlays/local
```

### 1.4 Verify

```bash
kubectl -n amcart-services get pods
kubectl -n amcart-services get svc
kubectl -n amcart-services logs -l app=product-service -f
```

Port-forward and call health:

```bash
kubectl -n amcart-services port-forward svc/product-service 5002:8080
curl http://localhost:5002/health
```

### 1.5 Remove

```bash
kubectl delete -k src/Services/ProductService/kubernetes/overlays/local
```

---

## Part 2: AWS EKS

### 2.1 Prerequisites

- **EKS cluster** (1.24+), **kubectl** configured.
- **PostgreSQL** in the cluster (e.g. in `amcart-databases`). If not, deploy it first:
  ```bash
  kubectl apply -k src/Databases/postgres/kubernetes/overlays/eks
  ```
- **Docker** (or Podman) and **AWS CLI** for building and pushing to ECR.

### 2.2 Create ECR repository

```bash
aws ecr create-repository --repository-name product-service --region <your-region>
```

Note the repository URI: `<account-id>.dkr.ecr.<region>.amazonaws.com/product-service`.

### 2.3 Build and push image

From **repository root** (build context: directory that contains `Services/`):

```bash
# Build (e.g. from src so Services/ exists)
cd src
docker build -f Services/ProductService/Dockerfile -t product-service:latest .

# Tag for ECR (replace ACCOUNT_ID and REGION)
docker tag product-service:latest ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/product-service:latest

# Log in to ECR and push
aws ecr get-login-password --region REGION | docker login --username AWS --password-stdin ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com
docker push ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/product-service:latest
```

### 2.4 Set image in EKS overlay

Edit **`src/Services/ProductService/kubernetes/overlays/eks/kustomization.yaml`** and set the image to your ECR URI:

```yaml
images:
  - name: product-service:latest
    newName: 123456789012.dkr.ecr.us-east-1.amazonaws.com/product-service   # your ECR URI
    newTag: latest
```

Replace `123456789012` and `us-east-1` with your account ID and region.

### 2.5 (Optional) Private ECR – imagePullSecret

If the EKS nodes don't have access to ECR (e.g. private cluster), create a secret and reference it in the deployment:

```bash
kubectl create secret docker-registry ecr-registry-secret \
  --namespace amcart-services \
  --docker-server=ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com \
  --docker-username=AWS \
  --docker-password=$(aws ecr get-login-password --region REGION)
```

Then in **`overlays/eks/deployment-patch.yaml`** add under `spec.template.spec`:

```yaml
imagePullSecrets:
  - name: ecr-registry-secret
```

### 2.6 Deploy ProductService

From the **repository root**:

```bash
kubectl apply -k src/Services/ProductService/kubernetes/overlays/eks
```

### 2.7 Verify

```bash
kubectl -n amcart-services get pods
kubectl -n amcart-services logs -l app=product-service -f
kubectl -n amcart-services port-forward svc/product-service 5002:8080
curl http://localhost:5002/health
```

### 2.8 Expose via Load Balancer (optional)

To expose ProductService outside the cluster, change the Service type or add an Ingress. Example – use a LoadBalancer service (overlay patch):

```yaml
apiVersion: v1
kind: Service
metadata:
  name: product-service
  namespace: amcart-services
spec:
  type: LoadBalancer
  ...
```

Or use an **Ingress** with an ALB Ingress Controller / AWS Load Balancer Controller and Ingress resource pointing to `product-service:8080`.

### 2.9 Tear down

```bash
kubectl delete -k src/Services/ProductService/kubernetes/overlays/eks
```

---

## Configuration (base manifests)

- **ConfigMap** (`product-service-config`): `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`, JWT Issuer/Audience.
- **Secret** (`product-service-secret`): `ConnectionStrings__DefaultConnection`, `Jwt__Secret`.

For production, replace the base Secret with values from a secret manager (e.g. AWS Secrets Manager + External Secrets Operator) and ensure `Jwt__Secret` is strong and not committed.

---

## API Gateway

The nginx API gateway routes product-related traffic to ProductService:

| Path prefix | Routes to |
|-------------|-----------|
| `/api/v1/products` | ProductService |
| `/api/v1/categories` | ProductService |
| `/api/v1/brands` | ProductService |
| `/api/v1/*` (other) | UserService |

See `src/Infrastructure/nginx/server-gateway.conf` for the full config.

---

## Summary

| Environment | Build context      | Image                    | Apply command |
|-------------|--------------------|---------------------------|----------------|
| Local K8s   | `src` (has Services) | `product-service:latest` (local) | `kubectl apply -k .../overlays/local` |
| EKS         | `src`              | ECR URI (set in overlay)   | `kubectl apply -k .../overlays/eks`   |

Both overlays assume Postgres is available at **`postgres.amcart-databases.svc.cluster.local:5432`** in the same cluster.
