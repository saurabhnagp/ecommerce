# Deploy UI on S3 + CDN and API via Nginx on AWS EKS (ALB)

Architecture:

- **Static UI:** S3 + CloudFront (or S3 website). SPA built with **empty** `VITE_USER_SERVICE_URL` so the browser requests **`/api/v1/...`** on the **same origin** as the site.
- **UI on S3:** Build with `npm run build` in **`Frontends/ui-app`** (or optional **`docker build`** there if you need a static+nginx image elsewhere).
- **Nginx API gateway:** **`src/Infrastructure/nginx`** (separate from UI). See **[Infrastructure/nginx/README.md](../../../Infrastructure/nginx/README.md)**.
- **Single origin:** CloudFront distribution with:
  - **Default behavior** → S3 (HTML/JS/CSS)
  - **Path pattern `/api/*`** → Origin = **ALB** in front of **amcart-gateway** (nginx) → **UserService**

This avoids **CORS**: the browser only talks to your CloudFront domain; nginx forwards `/api` to the cluster.

**UserService** does not enable CORS; only nginx/ALB is exposed for API traffic from the public web.

---

## 1. Build the UI for production

From **`src`**:

```bash
cd Frontends/ui-app
# Ensure no VITE_USER_SERVICE_URL (or empty) in .env.production
export VITE_USER_SERVICE_URL=
npm ci
npm run build
```

Confirm built assets use relative `/api/...` (no hard-coded API host in `dist/assets/*.js`).

Upload **`dist/`** to S3 and create a CloudFront distribution (origin = S3 bucket, OAI/OAC).

---

## 2. Build and push nginx gateway image to ECR

```bash
cd src
docker build -f Infrastructure/nginx/Dockerfile.gateway -t amcart-gateway:latest .

aws ecr get-login-password --region REGION | docker login --username AWS --password-stdin ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com
docker tag amcart-gateway:latest ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/amcart-gateway:latest
docker push ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/amcart-gateway:latest
```

---

## 3. Deploy gateway on EKS

Edit **`src/Infrastructure/nginx/kubernetes/gateway/overlays/eks/kustomization.yaml`**:

- Set `images[0].newName` to your ECR URI.
- Adjust replicas / imagePullSecrets if needed.

Apply:

```bash
kubectl apply -k src/Infrastructure/nginx/kubernetes/gateway/overlays/eks
```

The overlay sets the **Service** to **LoadBalancer** (NLB). Note the **external hostname** (or swap to **ALB** via AWS Load Balancer Controller + Ingress; see below).

---

## 4. CloudFront: second origin for `/api`

1. Add an **origin** pointing to the NLB DNS name (or ALB DNS).
2. Add a **behavior**:
   - **Path pattern:** `/api/*`
   - **Origin:** NLB/ALB origin
   - **Allowed HTTP methods:** GET, HEAD, OPTIONS, PUT, POST, PATCH, DELETE
   - **Cache policy:** CachingDisabled (or minimal cache for API)
   - **Origin request policy:** Forward all headers (especially `Authorization`, `Content-Type`)

3. **Default behavior** remains S3 for `/`, `/index.html`, static assets.

4. SPA **Alternate domain names (CNAME)** e.g. `www.example.com` — same domain for HTML and `/api`.

---

## 5. Optional: ALB Ingress instead of NLB Service

Many teams use **Ingress** + **AWS Load Balancer Controller** so the gateway Service stays ClusterIP and an **ALB** is created automatically. Example Ingress (adjust annotations for your cluster):

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: amcart-gateway
  namespace: amcart-gateway
  annotations:
    alb.ingress.kubernetes.io/scheme: internet-facing
    alb.ingress.kubernetes.io/target-type: ip
spec:
  ingressClassName: alb
  rules:
    - http:
        paths:
          - path: /api
            pathType: Prefix
            backend:
              service:
                name: amcart-gateway
                port:
                  number: 80
          - path: /health
            pathType: Prefix
            backend:
              service:
                name: amcart-gateway
                port:
                  number: 80
```

Use the **ALB DNS** as the CloudFront origin for `/api/*`.

---

## 6. UserService and Postgres on EKS

Deploy per existing docs:

- Postgres: `src/Databases/postgres/kubernetes/overlays/eks`
- UserService: `src/Services/UserService/kubernetes/overlays/eks`

Gateway upstream is **`user-service.amcart-services.svc.cluster.local:8080`** — ensure **UserService** runs in **`amcart-services`** with Service name **`user-service`**.

---

## 7. Checklist

- [ ] UI build with empty API base URL; uploaded to S3
- [ ] CloudFront: default → S3, `/api/*` → ALB/NLB
- [ ] Gateway image in ECR; EKS deployment healthy
- [ ] UserService reachable from gateway pods (`kubectl exec` curl)
- [ ] Browser: open site on CloudFront URL; login hits `/api/v1/auth/login` on same host

---

## 8. Tear down

```bash
kubectl delete -k src/Infrastructure/nginx/kubernetes/gateway/overlays/eks
```

Disable CloudFront behaviors and empty S3 as needed.
