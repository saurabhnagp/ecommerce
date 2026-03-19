# UserService – Deploy on Local Docker Desktop

Requirements and steps to run **UserService** in Docker on your machine.

---

## 1. Prerequisites

- **Docker Desktop** installed and running (Windows/Mac/Linux).
- **Build context:** The Dockerfile uses paths like `Services/UserService/...`, so the build context must be the directory that **contains** a `Services` folder (e.g. your `src` folder). From the repo root that would be:
  ```bash
  cd src
  docker build -f Services/UserService/Dockerfile -t user-service .
  ```

---

## 2. PostgreSQL Database

UserService uses **PostgreSQL** (Npgsql/EF Core). The app expects a connection string named **`DefaultConnection`**.

### Option A: Run PostgreSQL in Docker (recommended for local)

Start a Postgres container and create the database:

```bash
docker run -d --name amcart-postgres \
  -e POSTGRES_USER=amcart \
  -e POSTGRES_PASSWORD=amcart_password \
  -e POSTGRES_DB=amcart_users \
  -p 5432:5432 \
  postgres:16-alpine
```

Then apply migrations (see **Migrations** below).

### Option B: Use an existing PostgreSQL instance

Ensure it has:

- A database (e.g. `amcart_users`).
- User/password the app will use (e.g. `amcart` / `amcart_password`).
- Host/port reachable from the UserService container (use host DNS: `host.docker.internal` on Docker Desktop instead of `localhost`).

---

## 3. Configuration (environment variables)

When running the container, override config so it can reach the DB and listen on the right port.

| Variable | Purpose | Example |
|----------|---------|--------|
| **ConnectionStrings__DefaultConnection** | PostgreSQL connection string | See below |
| **ASPNETCORE_URLS** | Port the app listens on (Dockerfile exposes 8080) | `http://+:8080` |
| **Jwt__Secret** | JWT signing key (min 32 chars for HS256) | (use a strong secret) |
| **Jwt__Issuer** | Token issuer | `amcart` |
| **Jwt__Audience** | Token audience | `amcart-api` |
| **Jwt__AccessTokenExpiryMinutes** | Access token lifetime | `15` |
| **Jwt__RefreshTokenExpiryDays** | Refresh token lifetime | `7` |
| **App__BaseUrl** | Base URL for links/callbacks | `http://localhost:3000` |

**Connection string when DB is in Docker on same host:**

- From **host**: `Host=localhost;Port=5432;Database=amcart_users;Username=amcart;Password=amcart_password`
- From **another container** (e.g. UserService container): use `Host=host.docker.internal;Port=5432;...` (Docker Desktop) or the Postgres container name if you use a shared network.

---

## 4. Migrations

The app does not apply migrations automatically on startup. Before first run:

1. From the repo (or `src`), run EF migrations against the same connection string the container will use, for example:
   ```bash
   cd src/Services/UserService
   dotnet ef database update --project UserService.Infrastructure --startup-project UserService.Api
   ```
2. Or run the same from inside a one-off container that has the same connection string.

---

## 5. Build and run (summary)

1. **Start PostgreSQL** (if using Docker):
   ```bash
   docker run -d --name amcart-postgres \
     -e POSTGRES_USER=amcart -e POSTGRES_PASSWORD=amcart_password \
     -e POSTGRES_DB=amcart_users -p 5432:5432 postgres:16-alpine
   ```

2. **Apply migrations** (see above).

3. **Build image** (from `src` so `Services/UserService` exists):
   ```bash
   cd src
   docker build -f Services/UserService/Dockerfile -t user-service .
   ```

4. **Run container** (DB on host / host.docker.internal):
   ```bash
   docker run -d --name user-service -p 8080:8080 \
     -e ASPNETCORE_URLS=http://+:8080 \
     -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=amcart_users;Username=amcart;Password=amcart_password" \
     -e Jwt__Secret="amcart-user-service-secret-key-minimum-32-characters-long-for-hs256" \
     -e Jwt__Issuer=amcart \
     -e Jwt__Audience=amcart-api \
     user-service
   ```

5. **Verify:** `http://localhost:8080/health` (and Swagger if enabled in that environment).

---

## 6. Optional: docker-compose

You can put the above into a `docker-compose.yml` (e.g. in repo root or under `src`): define a `postgres` service and a `user-service` service that depends on it, set `ConnectionStrings__DefaultConnection` to the postgres service name (e.g. `Host=postgres;Port=5432;...`), and run migrations either via a one-off container or an init script.

---

## 7. Checklist

- [ ] Docker Desktop installed and running  
- [ ] PostgreSQL running and reachable (container or existing instance)  
- [ ] Database `amcart_users` created  
- [ ] Migrations applied  
- [ ] Image built with correct build context (`src` and `-f Services/UserService/Dockerfile`)  
- [ ] Container run with `ASPNETCORE_URLS=http://+:8080` and `ConnectionStrings__DefaultConnection` (and JWT/App settings if overridden)  
- [ ] Health check: `http://localhost:8080/health`
