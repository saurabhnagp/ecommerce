# Runbook: Secret Management

## Purpose
Procedures for managing secrets, credentials, and sensitive configuration.

## When to Use
- Rotating credentials
- Adding new secrets
- Investigating secret access
- Responding to credential compromise

## Prerequisites
- AWS Secrets Manager access
- kubectl access
- Security clearance for sensitive operations

---

## Section 1: AWS Secrets Manager

### 1.1 List Secrets

```bash
# List all secrets
aws secretsmanager list-secrets \
  --query 'SecretList[*].[Name,Description]' \
  --output table

# Filter by prefix
aws secretsmanager list-secrets \
  --filter Key=name,Values=amcart \
  --query 'SecretList[*].Name'
```

---

### 1.2 View Secret Value

⚠️ **CAUTION: Only view when necessary, actions are logged**

```bash
# Get secret value
aws secretsmanager get-secret-value \
  --secret-id amcart/prod/database \
  --query 'SecretString' \
  --output text | jq

# Get specific version
aws secretsmanager get-secret-value \
  --secret-id amcart/prod/database \
  --version-id "abc123" \
  --query 'SecretString'
```

---

### 1.3 Update Secret

```bash
# Update secret value
aws secretsmanager update-secret \
  --secret-id amcart/prod/database \
  --secret-string '{"username":"amcart","password":"NEW_PASSWORD_HERE","host":"amcart-prod.xxx.rds.amazonaws.com","port":"5432"}'

# Or update from file
aws secretsmanager update-secret \
  --secret-id amcart/prod/database \
  --secret-string file://new-secret.json
```

---

### 1.4 Create New Secret

```bash
# Create new secret
aws secretsmanager create-secret \
  --name amcart/prod/new-service \
  --description "Credentials for new service" \
  --secret-string '{"api_key":"xxx","api_secret":"yyy"}'

# Create with rotation
aws secretsmanager create-secret \
  --name amcart/prod/database-rotating \
  --description "Database credentials with rotation" \
  --secret-string '{"username":"amcart","password":"xxx"}' \
  --rotation-rules AutomaticallyAfterDays=30
```

---

## Section 2: Kubernetes Secrets

### 2.1 List Kubernetes Secrets

```bash
# List secrets in namespace
kubectl get secrets -n amcart-prod

# View secret metadata
kubectl describe secret user-service-secrets -n amcart-prod
```

---

### 2.2 View Secret Content

⚠️ **CAUTION: Be careful with sensitive data**

```bash
# Get secret in base64
kubectl get secret user-service-secrets -n amcart-prod -o jsonpath='{.data}'

# Decode specific key
kubectl get secret user-service-secrets -n amcart-prod \
  -o jsonpath='{.data.db-password}' | base64 -d
```

---

### 2.3 Create Kubernetes Secret

```bash
# From literal values
kubectl create secret generic user-service-secrets \
  -n amcart-prod \
  --from-literal=db-password='supersecret' \
  --from-literal=jwt-secret='anothersecret'

# From file
kubectl create secret generic user-service-secrets \
  -n amcart-prod \
  --from-file=./secrets.env

# From AWS Secrets Manager (using External Secrets Operator)
kubectl apply -f - <<EOF
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: user-service-secrets
  namespace: amcart-prod
spec:
  refreshInterval: 1h
  secretStoreRef:
    name: aws-secrets-manager
    kind: SecretStore
  target:
    name: user-service-secrets
  data:
  - secretKey: db-connection-string
    remoteRef:
      key: amcart/prod/database
      property: connection_string
EOF
```

---

### 2.4 Update Kubernetes Secret

```bash
# Patch specific value
kubectl patch secret user-service-secrets -n amcart-prod \
  -p '{"data":{"db-password":"'$(echo -n 'newpassword' | base64)'"}}'

# Replace entire secret
kubectl apply -f - <<EOF
apiVersion: v1
kind: Secret
metadata:
  name: user-service-secrets
  namespace: amcart-prod
type: Opaque
data:
  db-password: $(echo -n 'newpassword' | base64)
  jwt-secret: $(echo -n 'newsecret' | base64)
EOF
```

---

## Section 3: Credential Rotation

### 3.1 Database Password Rotation

**Step 1: Update in AWS Secrets Manager**
```bash
aws secretsmanager update-secret \
  --secret-id amcart/prod/database \
  --secret-string '{"username":"amcart","password":"NEW_PASSWORD"}'
```

**Step 2: Update in PostgreSQL**
```sql
-- Connect as admin
ALTER USER amcart WITH PASSWORD 'NEW_PASSWORD';
```

**Step 3: Update Kubernetes Secret**
```bash
kubectl patch secret user-service-secrets -n amcart-prod \
  -p '{"data":{"db-password":"'$(echo -n 'NEW_PASSWORD' | base64)'"}}'
```

**Step 4: Restart affected services**
```bash
kubectl rollout restart deployment/user-service -n amcart-prod
kubectl rollout restart deployment/product-service -n amcart-prod
kubectl rollout restart deployment/order-service -n amcart-prod
# ... other services using database
```

**Step 5: Verify connectivity**
```bash
kubectl logs deployment/user-service -n amcart-prod | grep -i "database\|connection"
```

---

### 3.2 API Key Rotation

```bash
# 1. Generate new API key (example: Razorpay)
# Done in provider's dashboard

# 2. Update in Secrets Manager
aws secretsmanager update-secret \
  --secret-id amcart/prod/razorpay \
  --secret-string '{"key_id":"NEW_KEY","key_secret":"NEW_SECRET"}'

# 3. Update Kubernetes secret
kubectl patch secret payment-service-secrets -n amcart-prod \
  -p '{"data":{"razorpay-key":"'$(echo -n 'NEW_KEY' | base64)'","razorpay-secret":"'$(echo -n 'NEW_SECRET' | base64)'"}}'

# 4. Restart payment service
kubectl rollout restart deployment/payment-service -n amcart-prod

# 5. Test payment flow
curl -X POST https://api.amcart.com/api/v1/payments/test
```

---

### 3.3 JWT Secret Rotation

⚠️ **WARNING: This will invalidate all existing tokens**

```bash
# 1. Generate new secret
NEW_JWT_SECRET=$(openssl rand -base64 64)

# 2. Update in Secrets Manager
aws secretsmanager update-secret \
  --secret-id amcart/prod/jwt \
  --secret-string "{\"secret\":\"$NEW_JWT_SECRET\"}"

# 3. Update Kubernetes secret
kubectl patch secret user-service-secrets -n amcart-prod \
  -p '{"data":{"jwt-secret":"'$(echo -n "$NEW_JWT_SECRET" | base64)'"}}'

# 4. Restart user service
kubectl rollout restart deployment/user-service -n amcart-prod

# Note: All users will need to re-login
```

---

## Section 4: Emergency Procedures

### 4.1 Credential Compromise Response

**Immediate actions (within 15 minutes):**

1. **Identify scope**
   ```bash
   # Check which secrets may be compromised
   # Review CloudTrail logs for secret access
   aws cloudtrail lookup-events \
     --lookup-attributes AttributeKey=ResourceName,AttributeValue=amcart/prod/database
   ```

2. **Rotate compromised credentials**
   ```bash
   # Follow rotation procedure for affected secrets
   # See sections 3.1, 3.2, 3.3 above
   ```

3. **Review access logs**
   ```bash
   # Check for unauthorized access
   kubectl logs deployment/user-service -n amcart-prod | grep -i "unauthorized\|forbidden"
   ```

4. **Notify security team**
   ```
   # Create security incident in PagerDuty
   # Follow incident response procedure
   ```

---

### 4.2 Revoke All Sessions

```bash
# Clear Redis sessions
kubectl exec -it deployment/cart-service -n amcart-prod -- \
  redis-cli -h amcart-redis.xxx.cache.amazonaws.com FLUSHDB

# All users will need to re-login
```

---

## Section 5: Audit and Compliance

### 5.1 View Secret Access Logs

```bash
# CloudTrail events for Secrets Manager
aws cloudtrail lookup-events \
  --lookup-attributes AttributeKey=EventName,AttributeValue=GetSecretValue \
  --start-time $(date -d '7 days ago' +%Y-%m-%dT%H:%M:%SZ) \
  --end-time $(date +%Y-%m-%dT%H:%M:%SZ)
```

---

### 5.2 Secret Rotation Schedule

| Secret | Rotation Frequency | Last Rotated | Owner |
|--------|-------------------|--------------|-------|
| Database Password | 90 days | YYYY-MM-DD | DBA |
| JWT Secret | On compromise | YYYY-MM-DD | Security |
| API Keys | 180 days | YYYY-MM-DD | DevOps |
| Service Account | 365 days | YYYY-MM-DD | DevOps |

---

### 5.3 Compliance Checklist

- [ ] All secrets stored in Secrets Manager (not in code)
- [ ] Kubernetes secrets synced from Secrets Manager
- [ ] Rotation schedule documented and followed
- [ ] Access logs retained for compliance period
- [ ] Principle of least privilege applied
- [ ] No secrets in environment variables directly

---

## Section 6: Best Practices

### Do's

- ✅ Use AWS Secrets Manager for all secrets
- ✅ Use External Secrets Operator to sync to Kubernetes
- ✅ Rotate secrets regularly
- ✅ Use different secrets per environment
- ✅ Audit secret access periodically
- ✅ Document all secret locations

### Don'ts

- ❌ Commit secrets to git
- ❌ Log secrets in application logs
- ❌ Share secrets via Slack/email
- ❌ Use the same secret across environments
- ❌ Hard-code secrets in Dockerfiles
- ❌ Leave default credentials

---

## Related Runbooks

- [01-Service-Health-Check](01-Service-Health-Check.md)
- [04-Incident-Response](04-Incident-Response.md)

