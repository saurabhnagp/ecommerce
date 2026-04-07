# AmCart API gateway (nginx only)

**Independent of the UI app.** The SPA image is built from **`Frontends/ui-app`** (see that folder’s `Dockerfile` + `docker/`).

This folder is for the **amcart-gateway** image: API reverse proxy when the UI is hosted on S3/CDN.

## Build (context = repo `src` directory)

```bash
cd src
docker build -f Infrastructure/nginx/Dockerfile.gateway -t amcart-gateway:latest .
```

## Files

| File | Purpose |
|------|---------|
| **Dockerfile.gateway** | Nginx-only image |
| **nginx-main.conf** | Main config (rate limits, security) |
| **server-gateway.conf** | `/api` proxy to UserService |
| **snippets/proxy-api.conf** | Shared proxy headers |

## Kubernetes (gateway)

```bash
kubectl apply -k src/Infrastructure/nginx/kubernetes/gateway/overlays/local
# EKS:
kubectl apply -k src/Infrastructure/nginx/kubernetes/gateway/overlays/eks
```
