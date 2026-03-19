# ui-app folder structure

## Summary

| Folder | Purpose |
|--------|---------|
| **`src/`** | React app (source). |
| **`nginx/`** | Nginx config for the **optional** `docker build` image (SPA + `/api` proxy). Use if you run the UI in a container; **production UI is S3** — usually you only need `npm run build` + upload `dist/`. |
| **`docs/`** | Deployment guides. |

**No `kubernetes/` folder here.** UI ships to **S3**. API gateway K8s manifests live under **`src/Infrastructure/nginx/kubernetes/gateway/`**.

---

## `nginx/`

Used only when you run **`docker build`** to produce **amcart-ui** (e.g. local container test). For **S3**, build static assets with **`npm run build`**; nginx in this folder is not involved in the S3 deploy.

---

## Outside ui-app

| Location | What |
|----------|------|
| **`src/Infrastructure/nginx/`** | **amcart-gateway** image + **kubernetes/gateway/** for K8s/EKS. |
