# Single EC2 deployment (AmCart UserService + UI)

Docker Compose stack: **PostgreSQL**, **UserService** (ECR image), **UI** (nginx + SPA, ECR image). Nginx in the UI container proxies `/api` to UserService; see [`nginx/default.conf`](nginx/default.conf).

## 1. AWS prerequisites (one-time)

### Billing (required)

1. In **AWS Billing → Billing preferences**, enable **Receive Billing Alerts**.
2. Open **CloudWatch → Alarms → Billing** and create an alarm on **EstimatedCharges** > e.g. **5 USD** (or your threshold).
3. Optional: **AWS Budgets** with email notification.

### ECR repositories

Create two repositories in your region (e.g. `us-east-1`):

- `amcart/user-service`
- `amcart/ui`

Note the **registry URL**: `<account_id>.dkr.ecr.<region>.amazonaws.com`.

### EC2 instance

1. Launch **Amazon Linux 2022** or **Ubuntu** (e.g. **t3.micro** Free Tier where eligible).
2. **Security group**: **SSH (22)** from **your IP** only; **HTTP (80)** from `0.0.0.0/0` (or restrict while testing).
3. Create or select a **key pair** for SSH.
4. Attach an **IAM instance role** with **AmazonEC2ContainerRegistryReadOnly** (and optionally **AmazonSSMManagedInstanceCore** if you use SSM instead of SSH).

### Install Docker on the instance (SSH)

**Amazon Linux 2022:**

```bash
sudo dnf update -y
sudo dnf install -y docker
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
# log out and back in for group docker
sudo dnf install -y docker-compose-plugin
```

**Ubuntu:**

```bash
sudo apt-get update -y
sudo apt-get install -y docker.io docker-compose-plugin
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
```

Install **AWS CLI v2** if not present (needed for `aws ecr get-login-password` on the instance).

### GitHub Actions → AWS (OIDC recommended)

1. In **IAM → Identity providers**, add **OpenID Connect** for `token.actions.githubusercontent.com` (audience `sts.amazonaws.com`).
2. Create an **IAM role** trusted by that provider, restricted to your repo, with policies such as:
   - **AmazonEC2ContainerRegistryPowerUser** (push images from CI), or tighter `ecr:BatchCheckLayerAvailability`, `ecr:GetDownloadUrlForLayer`, `ecr:BatchGetImage`, `ecr:PutImage`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`
3. Copy the role **ARN** into GitHub secret **`AWS_ROLE_ARN`**.

Alternative (simpler, less ideal): IAM user with access keys in **`AWS_ACCESS_KEY_ID`** / **`AWS_SECRET_ACCESS_KEY`**. Replace the OIDC step in [`.github/workflows/deploy-ec2.yml`](../../.github/workflows/deploy-ec2.yml) with the `aws-access-key-id` / `aws-secret-access-key` form (comment at top of that file).

### GitHub repository secrets (deploy workflow)

| Secret | Description |
|--------|-------------|
| `AWS_ROLE_ARN` | IAM role ARN for OIDC (or omit if using keys below) |
| `AWS_ACCESS_KEY_ID` | Optional; only if not using OIDC |
| `AWS_SECRET_ACCESS_KEY` | Optional; only if not using OIDC |
| `EC2_HOST` | Public IP or DNS of the instance |
| `EC2_USER` | e.g. `ec2-user` (AL2022) or `ubuntu` |
| `EC2_SSH_KEY` | Private key PEM (full contents) for deploy |

### GitHub repository variables (optional)

| Variable | Default | Description |
|----------|---------|-------------|
| `DEPLOY_PATH` | `/opt/amcart` | Directory on EC2 containing `docker-compose.yml` and `.env` |

## 2. First-time setup on EC2 (manual deploy)

```bash
sudo mkdir -p /opt/amcart
sudo chown $USER:$USER /opt/amcart
cd /opt/amcart
```

Copy this folder from the repo (or clone the repo and symlink):

```bash
# From your laptop (example)
scp -r infra/ec2/* ec2-user@YOUR_EC2_IP:/opt/amcart/
```

Create **`.env`** from [`.env.example`](.env.example) and set real values (`POSTGRES_PASSWORD`, `JWT_SECRET`, `ECR_REGISTRY`, image URIs).

Log in to ECR and start the stack:

```bash
cd /opt/amcart
set -a && source .env && set +a
aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$ECR_REGISTRY"
docker compose --env-file .env pull
docker compose --env-file .env up -d
```

**Smoke test:** open `http://YOUR_EC2_IP/` — register, sign in, open profile.

**Logs:** `docker compose -f /opt/amcart/docker-compose.yml logs -f`

## 3. Stop / Start to save costs (recommended over terminate)

**Stop** the instance when idle instead of **terminating** it. When stopped:
- **No compute charges** (you only pay ~$1.60/month for the 20 GiB EBS volume)
- Docker, configs, Postgres data, `.env` — **everything is preserved**
- Just start again when needed; containers auto-restart (`restart: unless-stopped`)

**Caveat**: The public IP changes on each start (unless you have an Elastic IP). The management script handles this automatically.

### Using the management script (from your laptop)

Edit the variables at the top of [`scripts/ec2-manage.sh`](scripts/ec2-manage.sh) to match your instance, then:

```bash
# Stop when done for the day (no compute bill)
bash infra/ec2/scripts/ec2-manage.sh stop

# Start next morning (updates GitHub secret EC2_HOST automatically)
bash infra/ec2/scripts/ec2-manage.sh start

# Check status
bash infra/ec2/scripts/ec2-manage.sh status

# SSH in
bash infra/ec2/scripts/ec2-manage.sh ssh
```

The `start` command waits for the instance to be running, gets the new IP, and updates the **`EC2_HOST`** GitHub secret via `gh` CLI (install: https://cli.github.com/).

### If you must terminate and recreate

1. When launching, paste [`scripts/user-data-bootstrap.sh`](scripts/user-data-bootstrap.sh) into **Advanced details → User data**. This auto-installs Docker + Compose on first boot.
2. After launch, SCP files and create `.env` as in section 2 above.
3. Update `INSTANCE_ID` in `ec2-manage.sh` and `EC2_HOST` in GitHub secrets.

## 4. CI/CD

- **CI:** [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) — `dotnet test` + UI `npm run build` on PR/push.
- **Deploy:** [`.github/workflows/deploy-ec2.yml`](../../.github/workflows/deploy-ec2.yml) — on push to `main`, build/push images to ECR, SSH to EC2, `docker compose pull` + `up -d` with **`GITHUB_SHA`** image tags.

Ensure ECR repositories exist before the first deploy run.

## 5. Local Docker build commands (from repo root)

```bash
docker build -f src/Services/UserService/Dockerfile -t user-service:local src
docker build -f src/Frontends/ui-app/Dockerfile -t ui:local src/Frontends/ui-app
```

The UI build uses empty `VITE_USER_SERVICE_URL` so the browser calls same-origin `/api` (proxied by nginx).
