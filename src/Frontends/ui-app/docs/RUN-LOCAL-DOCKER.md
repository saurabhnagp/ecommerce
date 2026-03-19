# Run the UI in Docker (local) → UserService + ProductService

The SPA calls **`/api/...`** on the same origin as the page. The UI container runs **nginx** and forwards:

| Path prefix | Backend |
|-------------|---------|
| `/api/v1/products`, `/api/v1/brands`, `/api/v1/categories` | ProductService |
| Everything else under `/api/` (e.g. `/api/v1/auth`, `/api/v1/users`) | UserService |

---

## Option A — Backends on your machine (`Dockerfile.local`)

Nginx uses **`host.docker.internal:5001`** (UserService) and **`:5002`** (ProductService). Works on **Docker Desktop** (Windows/Mac). Linux: use `--add-host=host.docker.internal:host-gateway` on `docker run`, or put all services on a Docker network (Option B).

### 1. PostgreSQL

```bash
cd src/Databases/postgres
docker compose up -d
```

Create DBs / run migrations if you have not already.

### 2. UserService (terminal 1)

```bash
cd src/Services/UserService
dotnet ef database update --project UserService.Infrastructure --startup-project UserService.Api
dotnet run --project UserService.Api
```

Default URL: **http://localhost:5001** (check console).

### 3. ProductService (terminal 2)

```bash
cd src/Services/ProductService
dotnet ef database update --project ProductService.Infrastructure --startup-project ProductService.Api
dotnet run --project ProductService.Api
```

Default URL: **http://localhost:5002** (check `launchSettings`).

### 4. Build and run the UI container

```bash
cd src/Frontends/ui-app
docker build -f Dockerfile.local -t amcart-ui:local .
docker run --rm -p 8080:80 amcart-ui:local
```

**Linux** (if `host.docker.internal` fails):

```bash
docker run --rm -p 8080:80 --add-host=host.docker.internal:host-gateway amcart-ui:local
```

### 5. Open the app

**http://localhost:8080** — sign-in, register, and future product pages use `/api/...` through nginx.

---

## Option B — All services in Docker (same network)

1. Create a network: `docker network create amcart-local`
2. Run Postgres on that network (name e.g. `postgres`).
3. Build/run **UserService** and **ProductService** images on the same network with container names **`user-service`** and **`product-service`**, listening on **8080** inside the container.
4. Before building the UI, copy a custom nginx default that uses:

   ```nginx
   upstream user_service { server user-service:8080; }
   upstream product_service { server product-service:8080; }
   ```

   Or adjust **`nginx/default.local-docker.conf`** to those server names and `docker build -f Dockerfile.local`.

5. Run the UI container on **`amcart-local`**:

   ```bash
   docker run --rm -p 8080:80 --network amcart-local amcart-ui:local
   ```

---

## Option C — Default `Dockerfile` (Kubernetes-style DNS)

`docker build -t amcart-ui:latest .` (without `.local`) uses **`nginx/default.conf`** with upstreams:

- `user-service.amcart-services.svc.cluster.local:8080`
- `product-service.amcart-services.svc.cluster.local:8080`

Use that image **inside a cluster** where those Services exist, not on plain Docker Desktop without those DNS names.

---

## Checklist

- [ ] Postgres up; UserService + ProductService DBs migrated  
- [ ] UserService on **5001**, ProductService on **5002** (Option A)  
- [ ] UI built with **`Dockerfile.local`**, run on **8080**  
- [ ] Browser: **http://localhost:8080**

---

## Troubleshooting

| Issue | What to check |
|-------|----------------|
| 502 on login | UserService listening on 5001; from host `curl http://localhost:5001/health` |
| 502 on products | ProductService on 5002; `curl http://localhost:5002/api/v1/products` |
| `host.docker.internal` fails | Linux: `host-gateway` (above) or Option B |
