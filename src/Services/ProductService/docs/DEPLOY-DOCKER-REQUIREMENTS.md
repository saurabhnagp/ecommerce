# ProductService – Deploy on Local Docker Desktop

Requirements and steps to run **ProductService** in Docker on your machine.

---

## 1. Prerequisites

- **Docker Desktop** installed and running (Windows/Mac/Linux).
- **Build context:** The Dockerfile uses paths like `Services/ProductService/...`, so the build context must be the directory that **contains** a `Services` folder (e.g. your `src` folder). From the repo root that would be:
  ```bash
  cd src
  docker build -f Services/ProductService/Dockerfile -t product-service .
  ```

---

## 2. PostgreSQL Database

ProductService uses **PostgreSQL** (Npgsql/EF Core). The app expects a connection string named **`DefaultConnection`**.

### Option A: Run PostgreSQL in Docker (recommended for local)

Start a Postgres container and create the database:

```bash
docker run -d --name amcart-postgres \
  -e POSTGRES_USER=amcart \
  -e POSTGRES_PASSWORD=amcart_password \
  -e POSTGRES_DB=amcart_products \
  -p 5432:5432 \
  postgres:16-alpine
```

If you are also running UserService, you likely already have a Postgres instance. Create the `amcart_products` database inside it:

```bash
docker exec -it amcart-postgres psql -U amcart -d postgres -c "CREATE DATABASE amcart_products OWNER amcart;"
```

### Option B: Use an existing PostgreSQL instance

Ensure it has:

- A database (e.g. `amcart_products`).
- User/password the app will use (e.g. `amcart` / `amcart_password`).
- Host/port reachable from the ProductService container (use host DNS: `host.docker.internal` on Docker Desktop instead of `localhost`).

---

## 3. Configuration (environment variables)

When running the container, override config so it can reach the DB and listen on the right port.

| Variable | Purpose | Example |
|----------|---------|--------|
| **ConnectionStrings__DefaultConnection** | PostgreSQL connection string | See below |
| **ASPNETCORE_URLS** | Port the app listens on (Dockerfile exposes 8080) | `http://+:8080` |
| **Jwt__Secret** | JWT signing key (min 32 chars for HS256, must match UserService) | (use a strong secret) |
| **Jwt__Issuer** | Token issuer (must match UserService) | `amcart` |
| **Jwt__Audience** | Token audience (must match UserService) | `amcart-api` |

**Connection string when DB is in Docker on same host:**

- From **host**: `Host=localhost;Port=5432;Database=amcart_products;Username=amcart;Password=amcart_password`
- From **another container** (e.g. ProductService container): use `Host=host.docker.internal;Port=5432;...` (Docker Desktop) or the Postgres container name if you use a shared network.

---

## 4. Migrations

The app applies migrations automatically on startup (code-first auto-migrate). You do **not** need to run `dotnet ef database update` manually. When the container starts, any pending EF Core migrations will be applied to the `amcart_products` database.

If you prefer to run migrations manually:

```bash
cd src/Services/ProductService
dotnet ef database update --project ProductService.Infrastructure --startup-project ProductService.Api
```

---

## 5. Build and run (summary)

1. **Start PostgreSQL** (if using Docker):
   ```bash
   docker run -d --name amcart-postgres \
     -e POSTGRES_USER=amcart -e POSTGRES_PASSWORD=amcart_password \
     -e POSTGRES_DB=amcart_products -p 5432:5432 postgres:16-alpine
   ```

2. **Build image** (from `src` so `Services/ProductService` exists):
   ```bash
   cd src
   docker build -f Services/ProductService/Dockerfile -t product-service .
   ```

3. **Run container** (DB on host / host.docker.internal):
   ```bash
   docker run -d --name product-service -p 5002:8080 \
     -e ASPNETCORE_URLS=http://+:8080 \
     -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=amcart_products;Username=amcart;Password=amcart_password" \
     -e Jwt__Secret="amcart-user-service-secret-key-minimum-32-characters-long-for-hs256" \
     -e Jwt__Issuer=amcart \
     -e Jwt__Audience=amcart-api \
     product-service
   ```

4. **Verify:** `http://localhost:5002/health` (and Swagger at `http://localhost:5002/swagger` if in Development environment).

---

## 6. Optional: docker-compose

For a full-stack local setup, see **`infra/ec2/docker-compose.yml`** which includes Postgres, UserService, ProductService, and the UI. Copy to your machine, create `.env` from `.env.example`, and:

```bash
docker compose --env-file .env up -d
```

---

## 7. Checklist

- [ ] Docker Desktop installed and running
- [ ] PostgreSQL running and reachable (container or existing instance)
- [ ] Database `amcart_products` created
- [ ] Image built with correct build context (`src` and `-f Services/ProductService/Dockerfile`)
- [ ] Container run with `ASPNETCORE_URLS=http://+:8080` and `ConnectionStrings__DefaultConnection` (and JWT settings matching UserService)
- [ ] Health check: `http://localhost:5002/health`
