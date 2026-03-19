# Runbook: Deployment Procedures

## Purpose
Standard procedures for deploying and rolling back AmCart microservices.

## When to Use
- Regular releases
- Hotfix deployments
- Emergency rollbacks
- Canary deployments

## Prerequisites
- GitHub Actions access (or CI/CD tool)
- kubectl access to EKS
- Helm CLI installed
- Approval from team lead (production)

---

## Section 1: Standard Deployment

### 1.1 Pre-Deployment Checklist

- [ ] Code reviewed and approved
- [ ] All tests passing
- [ ] Docker image built and pushed to ECR
- [ ] Release notes prepared
- [ ] Team notified in Slack
- [ ] Monitoring dashboards open
- [ ] Rollback plan understood

---

### 1.2 Deploy via CI/CD (Recommended)

```yaml
# Trigger deployment via GitHub Actions
# 1. Go to Actions tab in GitHub
# 2. Select "Deploy to Production" workflow
# 3. Click "Run workflow"
# 4. Select service and version
# 5. Confirm deployment

# Or via git tag
git tag v1.2.3-user-service
git push origin v1.2.3-user-service
```

---

### 1.3 Manual Deployment (kubectl)

```bash
# Set context
kubectl config use-context amcart-prod

# Update deployment image
kubectl set image deployment/user-service \
  user-service=123456789.dkr.ecr.us-east-1.amazonaws.com/amcart/user-service:v1.2.3 \
  -n amcart-prod

# Watch rollout
kubectl rollout status deployment/user-service -n amcart-prod

# Verify deployment
kubectl get pods -n amcart-prod -l app=user-service
```

---

### 1.4 Helm Deployment

```bash
# Update Helm chart values
helm upgrade user-service ./charts/user-service \
  --namespace amcart-prod \
  --set image.tag=v1.2.3 \
  --set replicas=3 \
  --wait \
  --timeout 5m

# Check release status
helm status user-service -n amcart-prod

# View release history
helm history user-service -n amcart-prod
```

---

## Section 2: Rollback Procedures

### 2.1 Quick Rollback (kubectl)

```bash
# View rollout history
kubectl rollout history deployment/user-service -n amcart-prod

# Rollback to previous version
kubectl rollout undo deployment/user-service -n amcart-prod

# Rollback to specific revision
kubectl rollout undo deployment/user-service -n amcart-prod --to-revision=5

# Verify rollback
kubectl rollout status deployment/user-service -n amcart-prod
```

---

### 2.2 Helm Rollback

```bash
# View release history
helm history user-service -n amcart-prod

# Rollback to previous release
helm rollback user-service -n amcart-prod

# Rollback to specific revision
helm rollback user-service 3 -n amcart-prod

# Verify rollback
helm status user-service -n amcart-prod
```

---

### 2.3 Emergency Rollback Script

```bash
#!/bin/bash
# emergency-rollback.sh

SERVICE=$1
NAMESPACE=${2:-amcart-prod}

if [ -z "$SERVICE" ]; then
    echo "Usage: ./emergency-rollback.sh <service-name> [namespace]"
    exit 1
fi

echo "🚨 EMERGENCY ROLLBACK: $SERVICE"
echo "Rolling back in 5 seconds... Press Ctrl+C to cancel"
sleep 5

kubectl rollout undo deployment/$SERVICE -n $NAMESPACE
kubectl rollout status deployment/$SERVICE -n $NAMESPACE --timeout=3m

if [ $? -eq 0 ]; then
    echo "✅ Rollback completed successfully"
else
    echo "❌ Rollback failed - escalate immediately"
    exit 1
fi
```

---

## Section 3: Canary Deployment

### 3.1 Setup Canary

```bash
# Deploy canary (10% traffic)
kubectl apply -f - <<EOF
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service-canary
  namespace: amcart-prod
  labels:
    app: user-service
    track: canary
spec:
  replicas: 1
  selector:
    matchLabels:
      app: user-service
      track: canary
  template:
    metadata:
      labels:
        app: user-service
        track: canary
    spec:
      containers:
      - name: user-service
        image: 123456789.dkr.ecr.us-east-1.amazonaws.com/amcart/user-service:v1.2.3-canary
        ports:
        - containerPort: 5001
EOF
```

---

### 3.2 Monitor Canary

```bash
# Watch canary pods
kubectl get pods -n amcart-prod -l track=canary

# Check canary logs
kubectl logs -f deployment/user-service-canary -n amcart-prod

# Compare metrics (Grafana)
# - Error rate: canary vs stable
# - Latency: canary vs stable
# - Request rate: canary vs stable
```

---

### 3.3 Promote or Rollback Canary

**Promote (success):**
```bash
# Update stable to canary version
kubectl set image deployment/user-service \
  user-service=123456789.dkr.ecr.us-east-1.amazonaws.com/amcart/user-service:v1.2.3-canary \
  -n amcart-prod

# Delete canary
kubectl delete deployment user-service-canary -n amcart-prod
```

**Rollback (failure):**
```bash
# Delete canary deployment
kubectl delete deployment user-service-canary -n amcart-prod

# Stable continues serving traffic
```

---

## Section 4: Blue-Green Deployment

### 4.1 Deploy Green Environment

```bash
# Deploy green (new version)
helm install user-service-green ./charts/user-service \
  --namespace amcart-prod \
  --set image.tag=v1.2.3 \
  --set service.name=user-service-green

# Verify green is healthy
kubectl get pods -n amcart-prod -l app=user-service-green
```

---

### 4.2 Switch Traffic

```bash
# Update service to point to green
kubectl patch service user-service -n amcart-prod -p '
{
  "spec": {
    "selector": {
      "app": "user-service-green"
    }
  }
}'
```

---

### 4.3 Cleanup Blue

```bash
# After verification, delete blue
helm uninstall user-service-blue -n amcart-prod
```

---

## Section 5: Database Migrations

### 5.1 Pre-Migration Checklist

- [ ] Backup database (see Database Operations runbook)
- [ ] Migration tested in staging
- [ ] Rollback migration prepared
- [ ] Off-peak hours

---

### 5.2 Run Migration

```bash
# Connect to migration job
kubectl exec -it job/user-service-migration -n amcart-prod -- \
  dotnet ef database update

# Or via custom job
kubectl apply -f - <<EOF
apiVersion: batch/v1
kind: Job
metadata:
  name: user-service-migration-$(date +%Y%m%d%H%M%S)
  namespace: amcart-prod
spec:
  template:
    spec:
      containers:
      - name: migration
        image: 123456789.dkr.ecr.us-east-1.amazonaws.com/amcart/user-service:v1.2.3
        command: ["dotnet", "ef", "database", "update"]
        envFrom:
        - secretRef:
            name: user-service-secrets
      restartPolicy: Never
  backoffLimit: 0
EOF

# Watch migration
kubectl logs -f job/user-service-migration-YYYYMMDDHHMMSS -n amcart-prod
```

---

### 5.3 Rollback Migration

```bash
# Rollback to specific migration
kubectl exec -it deployment/user-service -n amcart-prod -- \
  dotnet ef database update PreviousMigrationName

# Or restore from backup (last resort)
# See Database Operations runbook
```

---

## Section 6: Post-Deployment

### 6.1 Verification Checklist

- [ ] All pods running
- [ ] Health checks passing
- [ ] No errors in logs
- [ ] Metrics normal in Grafana
- [ ] Smoke tests passing
- [ ] No increase in error rate

---

### 6.2 Smoke Tests

```bash
# Run smoke tests
./scripts/smoke-tests.sh production

# Manual verification
curl -s https://api.amcart.com/api/v1/users/health
curl -s https://api.amcart.com/api/v1/products?limit=1
```

---

### 6.3 Notify Team

```bash
# Update deployment tracker
# Post in #deployments Slack channel

# Example message:
# ✅ Deployed user-service v1.2.3 to production
# Changes: https://github.com/amcart/user-service/releases/tag/v1.2.3
# All health checks passing
```

---

## Deployment Schedule

| Environment | Allowed Times | Approval |
|-------------|---------------|----------|
| Development | Anytime | Self |
| Staging | Anytime | Self |
| Production | Mon-Thu 10:00-16:00 UTC | Team Lead |
| Production (hotfix) | Anytime | Engineering Lead |

---

## Related Runbooks

- [01-Service-Health-Check](01-Service-Health-Check.md)
- [02-Database-Operations](02-Database-Operations.md)
- [04-Incident-Response](04-Incident-Response.md)

