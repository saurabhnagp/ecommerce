# Deploy PostgreSQL on Local Docker Desktop

This guide covers running **PostgreSQL** for AmCart (UserService + ProductService) on your local machine using **Docker Desktop** with **persistent storage**.

---

## Prerequisites

- **Docker Desktop** installed and running.
- No other process using **port 5432** (or set `POSTGRES_PORT` to a different port).

---

## Option A: Docker Compose (recommended for local)

### 1. Navigate to the postgres config directory

```bash
cd src/Databases/postgres
```

### 2. (Optional) Create a `.env` file

Copy the example and adjust if needed:

```bash
cp .env.example .env
```

Default values in `.env.example` match the connection strings used by UserService and ProductService (`amcart` / `amcart_password`, databases `amcart_users` and `amcart_products`).

### 3. Start PostgreSQL with persistent storage

```bash
docker compose up -d
```

- **Persistent storage:** Data is stored in the Docker named volume `amcart-postgres-data`. It persists across `docker compose down` and restarts.
- **Init:** On first run, the container runs `init/init-multiple-dbs.sh` and creates the `amcart_products` database (the `amcart_users` database is created by the default `POSTGRES_DB`).

### 4. Verify

```bash
docker compose ps
docker compose exec postgres psql -U amcart -d amcart_users -c "\l"
```

You should see `amcart_users` and `amcart_products` in the list.

### 5. Connection strings for your apps

From the **host** (e.g. services running on your machine):

- UserService: `Host=localhost;Port=5432;Database=amcart_users;Username=amcart;Password=amcart_password`
- ProductService: `Host=localhost;Port=5432;Database=amcart_products;Username=amcart;Password=amcart_password`

From **another container** on the same Docker network, use hostname `postgres` and port `5432`. From a container not on the same Compose stack, use `Host=host.docker.internal;Port=5432;...` (Docker Desktop).

### 6. Stop and remove (data is kept in the volume)

```bash
docker compose down
```

To **remove the volume** as well (deletes all data):

```bash
docker compose down -v
```

---

## Option B: Kubernetes on Docker Desktop

If you use the **Kubernetes** feature in Docker Desktop and want to run PostgreSQL in the cluster:

### 1. Enable Kubernetes

In Docker Desktop: **Settings → Kubernetes → Enable Kubernetes**.

### 2. Deploy using Kustomize (local overlay)

From the **repository root**:

```bash
kubectl apply -k src/Databases/postgres/kubernetes/overlays/local
```

Or with kustomize built first:

```bash
kubectl kustomize src/Databases/postgres/kubernetes/overlays/local | kubectl apply -f -
```

### 3. Wait for the pod and check

```bash
kubectl -n amcart-databases get pods
kubectl -n amcart-databases get pvc
```

### 4. Connection from other pods in the cluster

Use the service DNS name:

- **Host:** `postgres.amcart-databases.svc.cluster.local`
- **Port:** `5432`
- **Connection string:** `Host=postgres.amcart-databases.svc.cluster.local;Port=5432;Database=amcart_users;Username=amcart;Password=amcart_password`

### 5. Port-forward to access from host (optional)

```bash
kubectl -n amcart-databases port-forward svc/postgres 5432:5432
```

Then use `Host=localhost;Port=5432;...` from apps on your machine.

### 6. Delete

```bash
kubectl delete -k src/Databases/postgres/kubernetes/overlays/local
```

---

## Summary

| Method              | Command / action                    | Data persistence              |
|---------------------|-------------------------------------|-------------------------------|
| Docker Compose      | `docker compose up -d` in postgres dir | Named volume `amcart-postgres-data` |
| K8s (Docker Desktop)| `kubectl apply -k .../overlays/local` | PVC (default StorageClass)    |

Use **Option A** for the simplest local setup with Docker only. Use **Option B** if you are developing or testing with Kubernetes locally.
