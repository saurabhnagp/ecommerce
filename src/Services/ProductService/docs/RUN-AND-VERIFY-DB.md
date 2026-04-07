# Run ProductService and Verify Database Connection

Use this when **PostgreSQL is running in Kubernetes** (e.g. `amcart-databases` namespace) and you want to run ProductService **on your machine** to test connection, health check, and tables.

**Migrations:** ProductService uses **code-first with auto-migrate on startup**. When the app starts, it applies any pending EF Core migrations, so you do **not** need to run `dotnet ef database update` manually.

---

## 1. Port-forward PostgreSQL to localhost

The in-cluster hostname `postgres.amcart-databases.svc.cluster.local` only resolves inside the cluster. When running ProductService locally, use a port-forward so `localhost:5432` reaches the Postgres pod:

```bash
kubectl -n amcart-databases port-forward svc/postgres 5432:5432
```

Leave this terminal open. In a **second terminal**, run the steps below.

---

## 2. Run ProductService

From the **repository root** or `src/Services/ProductService`:

```bash
cd src/Services/ProductService
dotnet run --project ProductService.Api
```

The app uses **Development** environment by default, so it reads `appsettings.Development.json` and connects to `Host=localhost;Port=5432;...` (the port-forward). On first run, it will apply pending migrations and create tables in `amcart_products`.

> **Note:** The Development connection string must point to `localhost`. If `appsettings.Development.json` doesn't override the connection string, you can set it via environment variable:
> ```bash
> set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=amcart_products;Username=amcart;Password=amcart_password
> dotnet run --project ProductService.Api
> ```

---

## 3. Health check

In a browser or another terminal:

```bash
curl http://localhost:5002/health
```

Or open: **http://localhost:5002/health**

- If the API and database are healthy, you get a **200** response (e.g. `Healthy`).
- If the database is unreachable, the health check reports **Unhealthy** (and may return 503 depending on your health endpoint config).

Swagger is available at **http://localhost:5002/swagger** in Development mode.

---

## 4. Verify tables in the database

**Option A – From your machine (with port-forward still running):**

```bash
docker run --rm -it --network host postgres:16-alpine psql -h localhost -p 5432 -U amcart -d amcart_products -c "\dt"
```

Or if you have `psql` installed:

```bash
psql -h localhost -p 5432 -U amcart -d amcart_products -c "\dt"
```

Password: `amcart_password`

**Option B – From inside the cluster:**

```bash
kubectl -n amcart-databases exec -it deploy/postgres -- psql -U amcart -d amcart_products -c "\dt"
```

You should see the ProductService tables (e.g. `Products`, `Categories`, `Brands`, `ProductReviews`, etc.).

---

## 5. Test API endpoints

Once the service is running, test key endpoints:

```bash
# List products (public)
curl http://localhost:5002/api/v1/products

# List categories (public)
curl http://localhost:5002/api/v1/categories

# List brands (public)
curl http://localhost:5002/api/v1/brands

# Detailed health with service info
curl http://localhost:5002/api/v1/health
```

Admin endpoints (create, update, delete) require a valid JWT with `role=admin`. Obtain a token from UserService first.

---

## Summary

| Step | Command / action |
|------|-------------------|
| 1 | `kubectl -n amcart-databases port-forward svc/postgres 5432:5432` (keep running) |
| 2 | `dotnet run --project ProductService.Api` (migrations run automatically on startup) |
| 3 | Open or `curl` `http://localhost:5002/health` |
| 4 | List tables: `psql` or `kubectl exec ... psql -c "\dt"` |
| 5 | Test: `curl http://localhost:5002/api/v1/products` |

- **Running locally:** Use port-forward + `appsettings.Development.json` (localhost).
- **Running in Kubernetes:** Use `appsettings.json` with `postgres.amcart-databases.svc.cluster.local` (no port-forward needed).
