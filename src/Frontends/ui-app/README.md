# AmCart UI App

Production UI is deployed to **S3** (see [docs/DEPLOY-EKS-S3-GATEWAY.md](docs/DEPLOY-EKS-S3-GATEWAY.md)). There is **no Kubernetes** manifest for this app under Frontends.

**Layout:** [docs/STRUCTURE.md](docs/STRUCTURE.md)

## Routes

| Path | Page |
|------|------|
| `/` | Home |
| `/login` | Sign in |
| `/register` | Register |
| `/forgot-password` | Forgot password |
| `/auth/reset-password?token=…` | Set new password (UserService email link) |
| `/reset-password?token=…` | Same page (alias) |
| `/account/change-password` | Change password (signed in) |

Configure UserService **`App:BaseUrl`** so password-reset emails point at your SPA, e.g. `https://yourdomain.com/reset-password?token=`.

## Development

```bash
cd src/Frontends/ui-app
npm install
npm run dev
```

Vite proxies **`/api`** → UserService (default `http://localhost:5001`).

## Production build for S3

```bash
npm run build
```

Upload **`dist/`** to S3. Route **`/api/*`** on CloudFront to your **API gateway** (EKS). The built app uses relative **`/api/...`** (same origin as the CloudFront domain).

## Optional: Docker image (not S3)

If you need a container with nginx + SPA (e.g. lab only):

```bash
docker build -t amcart-ui:latest .
```

Configs in **`nginx/`**.

## API gateway (Kubernetes)

**Not in this folder.** See **`src/Infrastructure/nginx`** and **`Infrastructure/nginx/kubernetes/gateway/`**.

## Docs

- [STRUCTURE.md](docs/STRUCTURE.md)
- [RUN-LOCAL-DOCKER.md](docs/RUN-LOCAL-DOCKER.md) — UI in Docker → UserService + ProductService on host
- [UI-DOCKER-K8S-USER-SERVICE.md](docs/UI-DOCKER-K8S-USER-SERVICE.md) — **UI in Docker + UserService in K8s** (nginx, no CORS)
- [DEPLOY-LOCAL-K8S.md](docs/DEPLOY-LOCAL-K8S.md) — local K8s **gateway** only
- [DEPLOY-EKS-S3-GATEWAY.md](docs/DEPLOY-EKS-S3-GATEWAY.md) — S3 + EKS gateway
