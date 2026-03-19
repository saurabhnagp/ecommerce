# AmCart Implementation Kickoff

Use this document when starting or continuing implementation in a new workspace. It points to the repo structure, key docs, and build order so you can proceed without prior chat context.

---

## 1. Tech Stack Summary

| Layer | Technology |
|-------|------------|
| **Frontend** | Nuxt.js 3, Vue 3, TypeScript, Tailwind CSS, Pinia |
| **Backend** | .NET 8, C# 12, ASP.NET Core Minimal APIs / Controllers |
| **API Gateway** | Nginx (reverse proxy) |
| **Databases** | PostgreSQL (Users, Orders, Payments, Product core), MongoDB/DocumentDB (Catalog, Reviews, Notifications), Redis (Cart, cache, sessions), OpenSearch (product search) |
| **Messaging** | RabbitMQ (Amazon MQ), MassTransit |
| **Auth** | Either AWS Cognito (see DAR-Cognito) or Custom User Service (see DAR-Custom + Technical-Design-Custom-User-Service-Auth) |
| **Deployment** | Docker, Kubernetes (EKS), Helm, Terraform |
| **CI/CD** | Jenkins (primary), optional GitHub Actions |

---

## 2. Repo Structure

```
amcart/
├── .github/workflows/          # Optional: GitHub Actions (ci.yml, cd.yml)
├── Jenkinsfile                 # Jenkins pipeline (build, test, Docker, deploy)
├── docker-compose.yml          # Full stack (services + frontend + nginx)
├── docker-compose.infrastructure.yml   # Infra only (Postgres, Redis, RabbitMQ, etc.)
├── docs/                       # This folder – all design and specs
├── frontend/                   # Nuxt 3 app
│   ├── app/, components/, pages/, composables/, stores/
│   ├── nuxt.config.ts, package.json
│   └── Dockerfile
├── src/                        # Backend .NET
│   ├── AmCart.sln
│   ├── ApiGateway/nginx/       # Nginx config (or use deploy/nginx)
│   ├── BuildingBlocks/
│   │   └── Common/             # Common.Logging, Common.Messaging, Common.Security
│   └── Services/
│       ├── UserService/        # .Api, .Application, .Domain, .Infrastructure + Dockerfile
│       ├── ProductService/
│       ├── CartService/
│       ├── OrderService/
│       ├── PaymentService/
│       ├── SearchService/
│       ├── NotificationService/
│       └── ReviewService/
├── deploy/
│   ├── docker/
│   ├── nginx/                  # nginx.conf, conf.d/
│   ├── k8s/                    # base/, overlays/dev|prod/
│   ├── helm/
│   └── terraform/
└── scripts/
    ├── run-all-services.sh
    ├── run-migrations.sh
    └── local-infra.sh
```

Inventory is part of **ProductService** (no separate InventoryService).

---

## 3. Key Docs (Reference These During Implementation)

| Doc | Purpose |
|-----|---------|
| **API-Specifications.md** | All API endpoints, request/response shapes, auth headers |
| **Database-Schema-User.md** | User/addresses/sessions schema (PostgreSQL); Cognito or custom auth variant |
| **Database-Schema-Product.md** | Product, categories, inventory, warehouses (PostgreSQL) |
| **Database-Schema-Product-NoSQL.md** | Product catalog in MongoDB/DocumentDB (alternative) |
| **Database-Schema-Cart.md** | Cart (PostgreSQL + Redis) |
| **Database-Schema-Order.md** | Orders (PostgreSQL) |
| **Database-Schema-Payment.md** | Payments (PostgreSQL) |
| **Technical-Design-Custom-User-Service-Auth.md** | Custom auth: schema deltas, flows, JWT, security (use if not Cognito) |
| **ADR/ADR-006-authentication.md** | JWT flows, bcrypt, refresh tokens, social login (C# examples) |
| **Deployment-Guide.md** | Local setup, Docker, EKS, migrations, env vars |
| **Project-Assumptions.md** | Business and technical assumptions |
| **DAR-*.md** | Decision rationale (frontend, backend, DB, Cognito vs custom, OpenSearch, etc.) |
| **ADR/** | Architecture decisions (microservices, auth, caching, EKS, etc.) |
| **Runbooks/** | Operations (health check, deployment, incidents, scaling, secrets) |

When asking Cursor or a colleague for implementation help, reference these with `@docs/<filename>` or by path.

---

## 4. Suggested Build Order

1. **Repo skeleton** – Create root folders (`src/`, `frontend/`, `deploy/`, `scripts/`), solution file, and placeholder projects if needed.
2. **Building blocks** – Implement `Common.Logging`, `Common.Messaging`, `Common.Security` (and optionally publish as NuGet packages for all services).
3. **User Service** – Auth (Cognito or custom per Technical-Design-Custom-User-Service-Auth), profile, addresses. Use [Database-Schema-User](Database-Schema-User.md) and [ADR-006-authentication](ADR/ADR-006-authentication.md).
4. **Product Service** – Products, categories, inventory, warehouses. Use [Database-Schema-Product](Database-Schema-Product.md).
5. **Cart Service** – Cart API + Redis. Use [Database-Schema-Cart](Database-Schema-Cart.md).
6. **Order Service** – Order creation, state machine, events. Use [Database-Schema-Order](Database-Schema-Order.md) and order flow diagrams in `docs/diagrams/`.
7. **Payment Service** – Payments, Razorpay integration. Use [Database-Schema-Payment](Database-Schema-Payment.md).
8. **Search Service** – Product search via OpenSearch. Use [DAR-OpenSearch-Selection](DAR-OpenSearch-Selection.md) and API spec.
9. **Notification Service** – Emails, optional SMS/push. Use API spec and event contracts.
10. **Review Service** – Reviews (e.g. MongoDB). Use API spec.
11. **Nginx** – Add `deploy/nginx/` config and wire routes to services.
12. **Frontend (Nuxt)** – Pages, composables, stores, call APIs via Nginx base URL.
13. **Docker & CI/CD** – Dockerfiles per service, `docker-compose.yml`, Jenkinsfile (or GitHub Actions), then K8s/Helm/Terraform as needed.

Adjust order (e.g. frontend stub early) as needed.

---

## 5. Docker and CI/CD Locations

| Item | Location |
|------|----------|
| **Service Dockerfile** | `src/Services/<ServiceName>/Dockerfile` (e.g. `src/Services/UserService/Dockerfile`) |
| **Build context for services** | `src/` (build from repo root: `docker build -f src/Services/UserService/Dockerfile src/`) |
| **Frontend Dockerfile** | `frontend/Dockerfile`, context `frontend/` |
| **Nginx config** | `deploy/nginx/` (nginx.conf, conf.d/) |
| **Compose** | `docker-compose.yml` and `docker-compose.infrastructure.yml` at repo root (or under `deploy/docker/`) |
| **K8s manifests** | `deploy/k8s/` (base/, overlays/) |
| **Helm charts** | `deploy/helm/` (optional) |
| **Terraform** | `deploy/terraform/` |
| **Jenkins pipeline** | `Jenkinsfile` at repo root |
| **GitHub Actions** | `.github/workflows/` (e.g. ci.yml, cd.yml) |

---

## 6. Building Blocks (Cross-Cutting Concerns)

- **Location:** `src/BuildingBlocks/Common/` – e.g. `Common.Logging`, `Common.Messaging`, `Common.Security`.
- **Consumption:** Services reference them as **project references** in the same solution, or as **NuGet packages** (e.g. `AmCart.Common.Logging`) from a private feed if you pack and publish them in CI.
- **Cross-cutting concerns** to implement here: logging (Serilog), messaging (MassTransit/RabbitMQ), JWT validation / security helpers, health checks, correlation ID. See ADRs and DAR-Backend-Technology-DotNet for patterns.

---

## 7. Quick Commands (from repo root)

```bash
# Restore and build backend
cd src && dotnet restore AmCart.sln && dotnet build

# Run migrations (example: User Service)
dotnet ef database update --project src/Services/UserService/UserService.Infrastructure --startup-project src/Services/UserService/UserService.Api

# Build a service image
docker build -t amcart/user-service:latest -f src/Services/UserService/Dockerfile src/

# Build frontend image
docker build -t amcart/frontend:latest -f frontend/Dockerfile frontend/

# Start infra only
docker-compose -f docker-compose.infrastructure.yml up -d

# Start full stack
docker-compose up -d
```

---

## 8. Auth Choice

- **Cognito:** See [DAR-Cognito-APIGateway-Selection](DAR-Cognito-APIGateway-Selection.md) and [Database-Schema-User](Database-Schema-User.md) (Cognito sync).
- **Custom User Service:** See [DAR-Custom-User-Service-Auth](DAR-Custom-User-Service-Auth.md) and [Technical-Design-Custom-User-Service-Auth](Technical-Design-Custom-User-Service-Auth.md) for schema deltas, flows, and security.

Use this kickoff doc plus the linked specs and ADRs as the single source of truth when implementing in any folder or workspace.
