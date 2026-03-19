# Nginx (UI Docker image)

| File | Used by | Purpose |
|------|---------|---------|
| **default.conf** | `Dockerfile` | K8s: UserService + ProductService cluster DNS. |
| **default.local-docker.conf** | `Dockerfile.local` | Local: backends on host ports **5001** / **5002**. |

See **[docs/RUN-LOCAL-DOCKER.md](../docs/RUN-LOCAL-DOCKER.md)**.
