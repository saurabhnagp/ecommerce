# ProductService – Kubernetes deployment

Deploy ProductService to **local Kubernetes** (Docker Desktop, minikube, kind) or **AWS EKS**.

## Prerequisites

- **PostgreSQL** running and reachable from the cluster (e.g. [amcart-databases](../../Databases/postgres/kubernetes) in namespace `amcart-databases`).
- **kubectl** configured for your cluster.
- The `amcart-services` namespace must exist (created by the UserService manifests or manually).

## Layout

- **`base/`** – Shared manifests (ConfigMap, Secret, Deployment, Service).
- **`overlays/local/`** – Local K8s: uses image `product-service:latest` (build locally).
- **`overlays/eks/`** – EKS: ECR image, 2 replicas, higher resources; set your ECR URL in `kustomization.yaml`.

## Quick start – Local Kubernetes

1. Build the image (from repo root; build context must contain `Services/`):

   ```bash
   cd src
   docker build -f Services/ProductService/Dockerfile -t product-service:latest .
   ```

2. Ensure Postgres is running in the cluster (see [Databases](../../Databases)). The `amcart_products` database must exist. Migrations are applied automatically when ProductService starts.

3. Deploy ProductService:

   ```bash
   kubectl apply -k src/Services/ProductService/kubernetes/overlays/local
   ```

4. Check and access:

   ```bash
   kubectl -n amcart-services get pods
   kubectl -n amcart-services port-forward svc/product-service 5002:8080
   # Then: curl http://localhost:5002/health
   ```

## EKS

1. Build and push the image to **Amazon ECR**.
2. Edit **`overlays/eks/kustomization.yaml`**: set `images[0].newName` to your ECR URI (e.g. `123456789012.dkr.ecr.us-east-1.amazonaws.com/product-service`).
3. If the repo is private, create an `imagePullSecret` and add it in `overlays/eks/deployment-patch.yaml`.
4. Deploy: `kubectl apply -k src/Services/ProductService/kubernetes/overlays/eks`.

## Database

ProductService uses PostgreSQL database `amcart_products`. The connection string is configured in:
- `base/secret.yaml` – points to `postgres.amcart-databases.svc.cluster.local`
- Override via environment variable `ConnectionStrings__DefaultConnection`

## JWT Authentication

ProductService validates JWTs issued by UserService. Both services share the same JWT secret, issuer, and audience. The secret is configured in `base/secret.yaml`.
