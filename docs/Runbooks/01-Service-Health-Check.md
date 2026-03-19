# Runbook: Service Health Check

## Purpose
Verify the health status of all AmCart microservices and dependent infrastructure.

## When to Use
- Routine health verification
- After deployments
- When alerts fire
- Before high-traffic events (sales)

## Prerequisites
- kubectl configured for EKS cluster
- Access to Grafana/CloudWatch
- On-call access

---

## Procedure

### Step 1: Check Kubernetes Cluster Status

```bash
# Set context to production
kubectl config use-context amcart-prod

# Check node status
kubectl get nodes

# Expected: All nodes in "Ready" state
```

**If nodes are NotReady:**
1. Check node events: `kubectl describe node <node-name>`
2. Check EC2 instance status in AWS Console
3. Escalate if multiple nodes affected

---

### Step 2: Check All Pod Status

```bash
# View all pods in production namespace
kubectl get pods -n amcart-prod -o wide

# Check for pods not in Running state
kubectl get pods -n amcart-prod | grep -v Running
```

**Expected states:**
- All pods: `Running`
- All containers: `1/1` or `2/2` (if sidecar)

**Pod Status Troubleshooting:**

| Status | Action |
|--------|--------|
| Pending | Check resources: `kubectl describe pod <pod>` |
| CrashLoopBackOff | Check logs: `kubectl logs <pod> --previous` |
| ImagePullBackOff | Check image name, ECR access |
| OOMKilled | Increase memory limits |

---

### Step 3: Verify Service Endpoints

```bash
# Check all services
kubectl get svc -n amcart-prod

# Test each service health endpoint
for svc in user-service product-service cart-service order-service payment-service inventory-service search-service notification-service review-service; do
  kubectl exec -it deployment/user-service -n amcart-prod -- \
    curl -s -o /dev/null -w "%{http_code}" http://$svc:500X/health
  echo " - $svc"
done
```

**Expected:** All services return `200`

---

### Step 4: Check External Health Endpoints

```bash
# API Gateway health
curl -s https://api.amcart.com/health
# Expected: {"status": "healthy"}

# Individual service health (via API Gateway)
curl -s https://api.amcart.com/api/v1/users/health
curl -s https://api.amcart.com/api/v1/products/health
curl -s https://api.amcart.com/api/v1/orders/health
```

---

### Step 5: Check Database Connectivity

#### PostgreSQL (RDS)

```bash
# Port forward to RDS
kubectl exec -it deployment/user-service -n amcart-prod -- \
  dotnet ef database health-check
  
# Or via psql
PGPASSWORD=$DB_PASSWORD psql -h amcart-prod.xxx.us-east-1.rds.amazonaws.com \
  -U amcart -d amcart_users -c "SELECT 1;"
```

#### Redis (ElastiCache)

```bash
# From within a pod
kubectl exec -it deployment/cart-service -n amcart-prod -- \
  redis-cli -h amcart-redis.xxx.cache.amazonaws.com PING
  
# Expected: PONG
```

#### MongoDB (DocumentDB)

```bash
# Connection test
kubectl exec -it deployment/review-service -n amcart-prod -- \
  mongosh "mongodb://amcart-docdb.xxx.docdb.amazonaws.com:27017/reviews" \
  --eval "db.runCommand({ping:1})"
```

#### OpenSearch

```bash
# Cluster health
curl -s https://amcart-opensearch.xxx.us-east-1.es.amazonaws.com/_cluster/health | jq
# Expected: status = "green" or "yellow"
```

---

### Step 6: Check Message Queue (RabbitMQ)

```bash
# Port forward to RabbitMQ management
kubectl port-forward svc/rabbitmq 15672:15672 -n amcart-prod &

# Check queue status
curl -s -u guest:guest http://localhost:15672/api/queues | jq '.[] | {name, messages}'
```

**Alert if:**
- Queue depth > 10,000 messages (potential consumer issue)
- Consumers = 0 for any queue

---

### Step 7: Check Grafana Dashboards

1. Open Grafana: https://grafana.internal.amcart.com
2. Navigate to "AmCart Overview" dashboard
3. Verify:
   - [ ] Request rate is normal
   - [ ] Error rate < 1%
   - [ ] P95 latency < 500ms
   - [ ] No service anomalies

---

### Step 8: Check CloudWatch Alarms

```bash
# List active alarms
aws cloudwatch describe-alarms --state-value ALARM --query 'MetricAlarms[*].[AlarmName,StateReason]'
```

**If alarms are active:**
1. Identify affected service
2. Follow corresponding runbook
3. Acknowledge alarm in CloudWatch

---

## Health Check Summary

| Component | Check | Expected |
|-----------|-------|----------|
| Kubernetes Nodes | kubectl get nodes | All Ready |
| Pods | kubectl get pods | All Running |
| Services | Health endpoints | 200 OK |
| PostgreSQL | SELECT 1 | Success |
| Redis | PING | PONG |
| MongoDB | ping | ok: 1 |
| OpenSearch | _cluster/health | green/yellow |
| RabbitMQ | Queue depths | < 10,000 |
| Grafana | Dashboard | No anomalies |
| CloudWatch | Alarms | No active alarms |

---

## Escalation

If any checks fail and cannot be resolved within 15 minutes:

1. **P1**: Multiple services down → Page Engineering Lead
2. **P2**: Single critical service down → Page On-call Secondary
3. **P3**: Minor degradation → Create incident ticket

---

## Related Runbooks

- [02-Database-Operations](02-Database-Operations.md) - For database issues
- [04-Incident-Response](04-Incident-Response.md) - For active incidents
- [07-Log-Analysis](07-Log-Analysis.md) - For debugging

