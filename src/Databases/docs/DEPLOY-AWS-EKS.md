# Deploy PostgreSQL on AWS EKS

This guide covers deploying **PostgreSQL** for AmCart on **AWS EKS** with **persistent storage** (EBS via the AWS EBS CSI driver) so it is suitable for production use.

---

## Prerequisites

- **AWS CLI** installed and configured with credentials that can create EKS and EBS resources.
- **kubectl** installed.
- An **EKS cluster** (1.24+) with:
  - OIDC provider configured (for IRSA, if you use it).
  - **EBS CSI driver** installed so that `StorageClass` `gp3` (or `gp2`) can provision volumes.
- **eksctl** or **Terraform** (optional) if you create the cluster from scratch.

---

## 1. Ensure EBS CSI driver is installed

EKS 1.23+ does not ship the EBS CSI driver by default. Add it so that PVCs with `storageClassName: gp3` get dynamic EBS volumes.

```bash
# Add the EBS CSI add-on (replace cluster name and region)
aws eks create-addon \
  --cluster-name <your-cluster-name> \
  --addon-name aws-ebs-csi-driver \
  --region <your-region>
```

Or use the [EBS CSI driver Helm chart](https://github.com/kubernetes-sigs/aws-ebs-csi-driver) or [IRSA setup](https://docs.aws.amazon.com/eks/latest/userguide/ebs-csi.html) as per AWS docs.

Verify that a `gp3` (or `gp2`) StorageClass exists:

```bash
kubectl get storageclass
```

If not, create a StorageClass that provisions EBS volumes (e.g. `gp3`).

---

## 2. Create the namespace (optional; Kustomize creates it)

The base Kustomization uses namespace `amcart-databases`. If you apply the EKS overlay, the namespace is created automatically. To create it manually:

```bash
kubectl create namespace amcart-databases
```

---

## 3. (Production) Replace default credentials

The base **Secret** in `kubernetes/base/secret.yaml` contains plain-text credentials. For production:

- Use **Sealed Secrets**, **External Secrets Operator** with AWS Secrets Manager, or another secret manager.
- Or at minimum: edit the secret, use a strong password, and apply from a secure pipeline (never commit production passwords).

Example: generate a base64 password and update the secret:

```bash
# Only for non-production or initial test
kubectl create secret generic postgres-credentials \
  --namespace amcart-databases \
  --from-literal=POSTGRES_USER=amcart \
  --from-literal=POSTGRES_PASSWORD='<strong-password>' \
  --from-literal=POSTGRES_DB=amcart_users \
  --dry-run=client -o yaml | kubectl apply -f -
```

Then remove or don’t apply the static `secret.yaml` from the repo.

---

## 4. Deploy PostgreSQL with the EKS overlay

From the **repository root**:

```bash
kubectl apply -k src/Databases/postgres/kubernetes/overlays/eks
```

Or build and apply in two steps:

```bash
kubectl kustomize src/Databases/postgres/kubernetes/overlays/eks -o postgres-eks.yaml
kubectl apply -f postgres-eks.yaml
```

The EKS overlay:

- Uses the **base** manifests (Namespace, ConfigMap, Secret, PVC, Deployment, Service).
- Patches the **PVC** with `storageClassName: gp3` for EBS provisioning.
- Optionally increases **memory** requests/limits for the Postgres container.

---

## 5. Verify deployment

```bash
kubectl -n amcart-databases get all
kubectl -n amcart-databases get pvc
kubectl -n amcart-databases get pods -w
```

Wait until the Postgres pod is **Running** and **Ready**. The first time, the init script creates the `amcart_products` database.

Optional: run a one-off pod to test connectivity:

```bash
kubectl -n amcart-databases run -it --rm psql --image=postgres:16-alpine --restart=Never -- \
  psql -h postgres -U amcart -d amcart_users -c "\l"
```

---

## 6. How other services connect

- **Inside the same EKS cluster:** Use the ClusterIP service:
  - **Host:** `postgres.amcart-databases.svc.cluster.local`
  - **Port:** `5432`
  - **UserService:** `Host=postgres.amcart-databases.svc.cluster.local;Port=5432;Database=amcart_users;Username=amcart;Password=<from-secret>`
  - **ProductService:** same host/port, `Database=amcart_products`.

- **From outside the cluster:** Expose the service (e.g. LoadBalancer/NodePort) or use **port-forward** for debugging:
  ```bash
  kubectl -n amcart-databases port-forward svc/postgres 5432:5432
  ```
  Then connect to `localhost:5432` with the same credentials.

---

## 7. Persistence and backups

- **Persistence:** Data is stored on an **EBS volume** (gp3) bound to the PVC. The Deployment uses `replicas: 1` and `strategy: Recreate` so the same PVC is reattached after pod restarts.
- **Backups:** For production, add:
  - Scheduled backups (e.g. `pg_dump` via CronJob or AWS Backup).
  - Optionally a standby or managed RDS for HA; this manifest is for a single-instance Postgres in EKS.

---

## 8. Scaling and high availability (optional)

For higher availability you would typically:

- Use **Amazon RDS for PostgreSQL** (managed, multi-AZ), or
- Run a Patroni/Stolon cluster in Kubernetes (beyond this guide).

This deployment is a **single-replica** Postgres suitable for dev/staging or low-risk production with backups.

---

## 9. Tear down

To remove the deployment and release the EBS volume:

```bash
kubectl delete -k src/Databases/postgres/kubernetes/overlays/eks
```

If the PVC is not deleted (e.g. `Retain` policy), delete it explicitly:

```bash
kubectl -n amcart-databases delete pvc postgres-data
```

---

## Checklist

- [ ] EKS cluster created and `kubectl` configured
- [ ] EBS CSI driver installed; `gp3` (or equivalent) StorageClass available
- [ ] Production credentials stored in a secret manager; Secret updated or replaced
- [ ] Applied EKS overlay: `kubectl apply -k src/Databases/postgres/kubernetes/overlays/eks`
- [ ] Pod Running and PVC Bound
- [ ] Connection strings for UserService and ProductService updated to use `postgres.amcart-databases.svc.cluster.local`
- [ ] Backup strategy in place for production
