# AmCart Databases

PostgreSQL configuration and deployment assets for AmCart services (UserService, ProductService).

## Layout

- **`postgres/`** – PostgreSQL
  - **`docker-compose.yml`** – Run Postgres locally with Docker Compose (persistent volume).
  - **`.env.example`** – Example env vars; copy to `.env` and adjust.
  - **`init/`** – Init scripts (e.g. create `amcart_products`); used by Docker and K8s.
  - **`kubernetes/`**
    - **`base/`** – Shared manifests (Namespace, ConfigMap, Secret, PVC, Deployment, Service). Apply as-is or via Kustomize.
    - **`overlays/local/`** – For local Kubernetes (Docker Desktop, minikube, kind). No storage class override.
    - **`overlays/eks/`** – For AWS EKS: `gp3` StorageClass, optional resource tweaks.

## Databases

| Database         | Used by        |
|-----------------|----------------|
| `amcart_users`  | UserService    |
| `amcart_products` | ProductService |

Default user/password in configs: `amcart` / `amcart_password` (change in production).

## Deployment guides

- **[Deploy on Local Docker Desktop](docs/DEPLOY-LOCAL-DOCKER.md)** – Docker Compose and optional in-cluster Postgres (Docker Desktop Kubernetes).
- **[Deploy on AWS EKS](docs/DEPLOY-AWS-EKS.md)** – EKS with persistent EBS storage and production considerations.

## Quick start (Docker Compose)

```bash
cd postgres
cp .env.example .env   # optional
docker compose up -d
```

Then point UserService and ProductService connection strings to `localhost:5432` with the databases above.
