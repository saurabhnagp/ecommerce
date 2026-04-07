# Local Kubernetes: API gateway (UI on S3 / local dev)

The **UI is not deployed on Kubernetes** — it is built for **S3** (see [DEPLOY-EKS-S3-GATEWAY.md](DEPLOY-EKS-S3-GATEWAY.md)) or run locally with **`npm run dev`**.

This guide covers **amcart-gateway** on Docker Desktop Kubernetes: nginx in front of UserService (same pattern as EKS/ALB).

---

## Prerequisites

- Docker Desktop with **Kubernetes** enabled.
- **PostgreSQL:** `kubectl apply -k src/Databases/postgres/kubernetes/overlays/local`
- **UserService:** `kubectl apply -k src/Services/UserService/kubernetes/overlays/local`
- **ProductService:** `kubectl apply -k src/Services/ProductService/kubernetes/overlays/local`

```bash
kubectl -n amcart-services get svc user-service product-service
```

---

## Build and deploy gateway

```bash
cd src
docker build -f Infrastructure/nginx/Dockerfile.gateway -t amcart-gateway:latest .
kubectl apply -k src/Infrastructure/nginx/kubernetes/gateway/overlays/local
```

Port-forward:

```bash
kubectl -n amcart-gateway port-forward svc/amcart-gateway 9081:80
```

- **http://localhost:9081/api/v1/products/...** → ProductService
- **http://localhost:9081/api/v1/categories/...** → ProductService
- **http://localhost:9081/api/v1/brands/...** → ProductService
- **http://localhost:9081/api/v1/...** (other) → UserService

---

## Local UI during development

- **Easiest:** `npm run dev` in `Frontends/ui-app` with Vite **`/api` proxy** → UserService on `localhost:5001` (no CORS).
- **Against gateway:** set `VITE_USER_SERVICE_URL=http://localhost:9081` — requires **CORS** on UserService unless you use a browser extension / same-origin setup.

---

## Remove gateway

```bash
kubectl delete -k src/Infrastructure/nginx/kubernetes/gateway/overlays/local
```

---

## Troubleshooting

| Issue | Check |
|-------|--------|
| 502 on `/api` | UserService/ProductService running; DNS from gateway pod to `user-service.amcart-services` / `product-service.amcart-services` |
