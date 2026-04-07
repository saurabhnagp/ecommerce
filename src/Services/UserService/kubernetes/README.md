# UserService – Kubernetes deployment

Deploy UserService to **local Kubernetes** (Docker Desktop, minikube, kind) or **AWS EKS**.

## Prerequisites

- **PostgreSQL** running and reachable from the cluster (e.g. [amcart-databases](../../Databases/postgres/kubernetes) in namespace `amcart-databases`).
- **kubectl** configured for your cluster.

## Layout

- **`base/`** – Shared manifests (Namespace, ConfigMap, Secret, Deployment, Service).
- **`overlays/local/`** – Local K8s: uses image `user-service:latest` (build locally).
- **`overlays/eks/`** – EKS: ECR image, 2 replicas, higher resources; set your ECR URL in `kustomization.yaml`.

## Quick start – Local Kubernetes

1. Build the image (from repo root; build context must contain `Services/`):

   ```bash
   cd src
   docker build -f Services/UserService/Dockerfile -t user-service:latest .
   ```

2. Ensure Postgres is running in the cluster (see [Databases](../../Databases)). Migrations are applied automatically when UserService starts (code-first, auto-migrate on startup).

3. Deploy UserService:

   ```bash
   kubectl apply -k src/Services/UserService/kubernetes/overlays/local
   ```

4. Check and access:

   ```bash
   kubectl -n amcart-services get pods
   kubectl -n amcart-services port-forward svc/user-service 8080:8080
   # Then: curl http://localhost:8080/health
   ```

## EKS

1. Build and push the image to **Amazon ECR**.
2. Edit **`overlays/eks/kustomization.yaml`**: set `images[0].newName` to your ECR URI (e.g. `123456789012.dkr.ecr.us-east-1.amazonaws.com/user-service`).
3. If the repo is private, create an `imagePullSecret` and add it in `overlays/eks/deployment-patch.yaml`.
4. Deploy: `kubectl apply -k src/Services/UserService/kubernetes/overlays/eks`.

See **[docs/DEPLOY-KUBERNETES.md](../docs/DEPLOY-KUBERNETES.md)** for detailed steps for both local and EKS.
