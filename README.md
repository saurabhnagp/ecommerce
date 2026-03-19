# AmCart (Ecommerce)

Monorepo with **UserService** (.NET), **ui-app** (Vite + React), and supporting **infra** for local Docker and **single-EC2** AWS deployment.

## What to push to GitHub

Commit **source, shared config, and CI workflows**. Typical paths:

| Include | Notes |
|--------|--------|
| `src/` | Services, frontends, databases docs, infrastructure nginx |
| `infra/` | EC2 compose, nginx override, scripts, `README.md`, `.env.example` |
| `.github/workflows/` | CI and deploy pipelines |
| `*.sln`, `*.csproj`, `package.json`, `Dockerfile`s | As present under `src/` |
| Root `.gitignore`, this `README.md` | |

**Do not commit:** `node_modules/`, `bin/`/`obj/`, `dist/`, real `.env` files, `.pem` keys, or `infra/ec2/.env` (see [.gitignore](.gitignore)).

## Quick links

- **EC2 deploy & AWS setup:** [infra/ec2/README.md](infra/ec2/README.md)
- **UI app (local dev):** [src/Frontends/ui-app/README.md](src/Frontends/ui-app/README.md)

## Local Docker builds (from repo root)

```bash
docker build -f src/Services/UserService/Dockerfile -t user-service:local src
docker build -f src/Frontends/ui-app/Dockerfile -t ui:local src/Frontends/ui-app
```

## CI/CD

- **CI:** `.github/workflows/ci.yml` — tests and builds on push/PR to `main` / `master`
- **Deploy:** `.github/workflows/deploy-ec2.yml` — push images to ECR and update EC2 (see `infra/ec2/README.md` for secrets)
