# UI in Docker + UserService in Kubernetes (Docker Desktop)

Goal: the **browser** only talks to **one origin** (the UI container on e.g. `http://localhost:8080`). **Nginx inside that container** forwards `/api/...` to **UserService running in K8s**. No CORS on UserService.

Because a normal **Docker container** cannot resolve Kubernetes DNS (`user-service.amcart-services.svc.cluster.local`), you expose UserService on your **host** with **kubectl port-forward**, then nginx uses **`host.docker.internal`** (Docker Desktop) to reach it.

```
Browser → http://localhost:8080/api/v1/auth/login
       → nginx (UI container) → host.docker.internal:5001
       → kubectl port-forward → UserService pod (K8s)
```

---

## Prerequisites

- Docker Desktop with **Kubernetes** enabled  
- **Postgres** + **UserService** already running in K8s (`amcart-services` namespace) and working  
- UI repo: `src/Frontends/ui-app`

---

## Step 1 — Confirm UserService in the cluster

```bash
kubectl -n amcart-services get pods
kubectl -n amcart-services get svc user-service
```

Note the **Service port** (often **8080** if the app listens on 8080 in the container).

---

## Step 2 — Port-forward UserService to your machine

In a **terminal you keep open**:

```bash
kubectl -n amcart-services port-forward svc/user-service 5001:8080
```

- **5001** = port on your **host** (pick any free port).  
- **8080** = port the **Service** targets (change if your Service uses another `targetPort`).

Check from the host:

```bash
curl http://localhost:5001/health
```

You should get a healthy response.

---

## Step 3 — Build the UI Docker image (local nginx config)

The **`Dockerfile.local`** image uses **`nginx/default.local-docker.conf`**, which sends **UserService** traffic to **`host.docker.internal:5001`**.

From **`host.docker.internal`**, “localhost” is **your Mac/Windows host**, where **port-forward** is listening — so the UI container reaches UserService.

```bash
cd src/Frontends/ui-app
docker build -f Dockerfile.local -t amcart-ui:local .
```

If your port-forward uses a port **other than 5001**, edit **`nginx/default.local-docker.conf`**:

```nginx
upstream user_service {
    server host.docker.internal:5001;   # change 5001 to match Step 2
}
```

Then rebuild.

**ProductService:** the same file uses **`host.docker.internal:5002`**. If you are not running ProductService locally, calls to `/api/v1/products` will fail until you port-forward ProductService to 5002 or remove those `location` blocks for pure UserService dev.

---

## Step 4 — Run the UI container

```bash
docker run --rm -p 8080:80 amcart-ui:local
```

**Linux** (if `host.docker.internal` is missing):

```bash
docker run --rm -p 8080:80 --add-host=host.docker.internal:host-gateway amcart-ui:local
```

---

## Step 5 — Develop in the browser

Open **http://localhost:8080**

- Sign in / register → `/api/v1/auth/...` → nginx → **5001** → K8s UserService.  
- Same origin for the browser → **no CORS**.

Keep **port-forward** (Step 2) running whenever you use the UI container.

---

## Daily workflow (short)

| Terminal | Command |
|----------|---------|
| 1 | `kubectl -n amcart-services port-forward svc/user-service 5001:8080` |
| 2 | `docker run --rm -p 8080:80 amcart-ui:local` |
| Browser | http://localhost:8080 |

After **UI code changes**, rebuild the image and run the container again:

```bash
docker build -f Dockerfile.local -t amcart-ui:local .
docker run --rm -p 8080:80 amcart-ui:local
```

For faster iteration without rebuilding every time, use **`npm run dev`** with Vite’s `/api` proxy to port-forwarded UserService instead (see main README).

---

## Alternative: run the UI **inside** Kubernetes

If you deploy the **amcart-ui** image as a **Pod** in the same cluster (same namespace or reachable network), nginx can use **cluster DNS** (`user-service.amcart-services.svc.cluster.local:8080`) in **`nginx/default.conf`** and the regular **`Dockerfile`**. Then you **do not** need port-forward for UserService. That is a different setup (Ingress/Service for the UI pod).

---

## Troubleshooting

| Problem | What to check |
|---------|----------------|
| 502 on login | Port-forward still running; `curl http://localhost:5001/health` on host |
| Connection refused from container | Docker Desktop: `host.docker.internal`. Linux: `--add-host=host.docker.internal:host-gateway` |
| Wrong port | Service `targetPort` vs port-forward second number (`5001:8080`) |
