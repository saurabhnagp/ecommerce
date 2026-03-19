# Run UserService and Verify Database Connection

Use this when **PostgreSQL is running in Kubernetes** (e.g. `amcart-databases` namespace) and you want to run UserService **on your machine** to test connection, health check, and tables.

**Migrations:** UserService uses **code-first with auto-migrate on startup**. When the app starts, it applies any pending EF Core migrations, so you do **not** need to run `dotnet ef database update` manually.

---

## 1. Port-forward PostgreSQL to localhost

The in-cluster hostname `postgres.amcart-databases.svc.cluster.local` only resolves inside the cluster. When running UserService locally, use a port-forward so `localhost:5432` reaches the Postgres pod:

```bash
kubectl -n amcart-databases port-forward svc/postgres 5432:5432
```

Leave this terminal open. In a **second terminal**, run the steps below.

---

## 2. Run UserService

From the **repository root** or `src/Services/UserService`:

```bash
cd src/Services/UserService
dotnet run --project UserService.Api
```

The app uses **Development** environment by default, so it reads `appsettings.Development.json` and connects to `Host=localhost;Port=5432;...` (the port-forward). On first run, it will apply pending migrations and create tables in `amcart_users`.

---

## 3. Health check

In a browser or another terminal:

```bash
curl http://localhost:5001/health
```

Or open: **http://localhost:5001/health**

- If the API and database are healthy, you get a **200** response (e.g. `Healthy`).
- If the database is unreachable, the health check reports **Unhealthy** (and may return 503 depending on your health endpoint config).

(If the app runs on a different port, check the console output for the URL, e.g. `http://localhost:5000`.)

---

## 4. Verify tables in the database

**Option A – From your machine (with port-forward still running):**

```bash
docker run --rm -it --network host postgres:16-alpine psql -h localhost -p 5432 -U amcart -d amcart_users -c "\dt"
```

Or if you have `psql` installed:

```bash
psql -h localhost -p 5432 -U amcart -d amcart_users -c "\dt"
```

Password: `amcart_password`

**Option B – From inside the cluster:**

```bash
kubectl -n amcart-databases exec -it deploy/postgres -- psql -U amcart -d amcart_users -c "\dt"
```

You should see the UserService tables (e.g. `Users`, `Addresses`, `RefreshTokens`, etc.).

---

## Summary

| Step | Command / action |
|------|-------------------|
| 1 | `kubectl -n amcart-databases port-forward svc/postgres 5432:5432` (keep running) |
| 2 | `dotnet run --project UserService.Api` (migrations run automatically on startup) |
| 3 | Open or `curl` `http://localhost:5001/health` |
| 4 | List tables: `psql` or `kubectl exec ... psql -c "\dt"` |

- **Running locally:** Use port-forward + `appsettings.Development.json` (localhost).
- **Running in Kubernetes:** Use `appsettings.json` with `postgres.amcart-databases.svc.cluster.local` (no port-forward needed).
