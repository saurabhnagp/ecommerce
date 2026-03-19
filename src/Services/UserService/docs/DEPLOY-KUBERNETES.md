# Deploy UserService on Local Kubernetes and AWS EKS

This guide covers deploying **UserService** to:

1. **Local Kubernetes** (Docker Desktop, minikube, kind)
2. **AWS EKS**

UserService expects **PostgreSQL** in the same cluster (namespace `amcart-databases`, service `postgres:5432`). Deploy the database first—see [Databases – Deploy](../../Databases/README.md).

**Migrations:** We use **EF Core code-first with auto-migrate on startup**. When the service starts, it applies any pending migrations to the database. You do **not** need to run `dotnet ef database update` manually before deploying.

---

## Part 1: Local Kubernetes (Docker Desktop / minikube / kind)

### 1.1 Prerequisites

- Kubernetes enabled (e.g. Docker Desktop → Settings → Kubernetes → Enable).
- PostgreSQL deployed in the cluster, e.g.:
  ```bash
  kubectl apply -k src/Databases/postgres/kubernetes/overlays/local
  ```

### 1.2 Build the Docker image

The Dockerfile expects build context to include a `Services/` directory. From the **repository root**:

```bash
cd src
docker build -f Services/UserService/Dockerfile -t user-service:latest .
```

For **minikube** (use the Docker daemon inside minikube so the image is available):

```bash
eval $(minikube docker-env)
cd src
docker build -f Services/UserService/Dockerfile -t user-service:latest .
```

### 1.3 Deploy UserService

From the **repository root**:

```bash
kubectl apply -k src/Services/UserService/kubernetes/overlays/local
```

### 1.4 Verify

```bash
kubectl -n amcart-services get pods
kubectl -n amcart-services get svc
kubectl -n amcart-services logs -l app=user-service -f
```

Port-forward and call health:

```bash
kubectl -n amcart-services port-forward svc/user-service 8080:8080
curl http://localhost:8080/health
```

### 1.5 Remove

```bash
kubectl delete -k src/Services/UserService/kubernetes/overlays/local
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
aws ecr create-repository --repository-name user-service --region <your-region>
```

Note the repository URI: `<account-id>.dkr.ecr.<region>.amazonaws.com/user-service`.

### 2.3 Build and push image

From **repository root** (build context: directory that contains `Services/`):

```bash
# Build (e.g. from src so Services/ exists)
cd src
docker build -f Services/UserService/Dockerfile -t user-service:latest .

# Tag for ECR (replace ACCOUNT_ID and REGION)
docker tag user-service:latest ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/user-service:latest

# Log in to ECR and push
aws ecr get-login-password --region REGION | docker login --username AWS --password-stdin ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com
docker push ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/user-service:latest
```

### 2.4 Set image in EKS overlay

Edit **`src/Services/UserService/kubernetes/overlays/eks/kustomization.yaml`** and set the image to your ECR URI:

```yaml
images:
  - name: user-service:latest
    newName: 123456789012.dkr.ecr.us-east-1.amazonaws.com/user-service   # your ECR URI
    newTag: latest
```

Replace `123456789012` and `us-east-1` with your account ID and region.

### 2.5 (Optional) Private ECR – imagePullSecret

If the EKS nodes don’t have access to ECR (e.g. private cluster), create a secret and reference it in the deployment:

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

### 2.6 Deploy UserService

From the **repository root**:

```bash
kubectl apply -k src/Services/UserService/kubernetes/overlays/eks
```

### 2.7 Verify

```bash
kubectl -n amcart-services get pods
kubectl -n amcart-services logs -l app=user-service -f
kubectl -n amcart-services port-forward svc/user-service 8080:8080
curl http://localhost:8080/health
```

### 2.8 Expose via Load Balancer (optional)

To expose UserService outside the cluster, change the Service type or add an Ingress. Example – use a LoadBalancer service (overlay patch):

```yaml
# In a new patch or in deployment-patch, you’d typically add a separate Service patch
apiVersion: v1
kind: Service
metadata:
  name: user-service
  namespace: amcart-services
spec:
  type: LoadBalancer
  ...
```

Or use an **Ingress** with an ALB Ingress Controller / AWS Load Balancer Controller and Ingress resource pointing to `user-service:8080`.

### 2.9 Tear down

```bash
kubectl delete -k src/Services/UserService/kubernetes/overlays/eks
```

---

## Configuration (base manifests)

- **ConfigMap** (`user-service-config`): `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`, JWT Issuer/Audience, token expiry, `App__BaseUrl`.
- **Secret** (`user-service-secret`): `ConnectionStrings__DefaultConnection`, `Jwt__Secret`.

For production, replace the base Secret with values from a secret manager (e.g. AWS Secrets Manager + External Secrets Operator) and ensure `Jwt__Secret` is strong and not committed.

---

## Summary

| Environment | Build context      | Image                    | Apply command |
|-------------|--------------------|---------------------------|----------------|
| Local K8s   | `src` (has Services) | `user-service:latest` (local) | `kubectl apply -k .../overlays/local` |
| EKS         | `src`              | ECR URI (set in overlay)   | `kubectl apply -k .../overlays/eks`   |

Both overlays assume Postgres is available at **`postgres.amcart-databases.svc.cluster.local:5432`** in the same cluster.
