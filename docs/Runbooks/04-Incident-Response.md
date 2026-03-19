# Runbook: Incident Response

## Purpose
Standard procedures for handling production incidents, from detection to resolution.

## When to Use
- Alert fires (PagerDuty)
- Customer reports issue
- Monitoring anomaly detected
- Service degradation observed

## Prerequisites
- PagerDuty access
- On-call rotation membership
- Access to all monitoring tools
- Communication channel access (Slack, etc.)

---

## Incident Severity Levels

| Level | Impact | Response Time | Examples |
|-------|--------|---------------|----------|
| **P1** | Site down, all customers affected | 15 min | API Gateway down, database outage |
| **P2** | Major feature broken | 30 min | Payments failing, search down |
| **P3** | Minor feature issue, workaround exists | 2 hours | Cart update slow, image upload failing |
| **P4** | Low impact, cosmetic | Next business day | UI glitch, minor performance issue |

---

## Section 1: Initial Response (First 5 Minutes)

### 1.1 Acknowledge Alert

```bash
# Acknowledge in PagerDuty (within 5 minutes)
# This notifies the team you're handling it
```

---

### 1.2 Assess Impact

**Quick checks:**

```bash
# Check API availability
curl -s -o /dev/null -w "%{http_code}" https://api.amcart.com/health

# Check pod status
kubectl get pods -n amcart-prod | grep -v Running

# Check error rate (CloudWatch/Grafana)
# Look for spikes in 5xx errors
```

---

### 1.3 Determine Severity

| Symptom | Severity |
|---------|----------|
| All API requests failing | P1 |
| Payment processing down | P1 |
| One service down but others work | P2 |
| Elevated error rate (>5%) | P2 |
| Slow responses (>2s) | P3 |
| Single feature degraded | P3 |

---

### 1.4 Open Incident Channel

```
# Create Slack channel: #incident-YYYYMMDD-brief-description
# Post initial status:

🚨 INCIDENT STARTED
Severity: P1
Issue: [Brief description]
Impact: [Who is affected]
Investigating: @your-name
Status: Investigating
```

---

## Section 2: Investigation

### 2.1 Identify Affected Service

```bash
# Check which services are unhealthy
for svc in user-service product-service cart-service order-service payment-service; do
  status=$(kubectl get pods -n amcart-prod -l app=$svc -o jsonpath='{.items[*].status.phase}')
  echo "$svc: $status"
done

# Check recent deployments
kubectl rollout history deployment --all-namespaces | grep amcart-prod
```

---

### 2.2 Check Logs

```bash
# Recent errors across services
kubectl logs -n amcart-prod -l app=user-service --since=15m | grep -i error | tail -50

# Specific service logs
kubectl logs -f deployment/user-service -n amcart-prod --tail=100

# Search in CloudWatch
aws logs filter-log-events \
  --log-group-name /amcart/prod/user-service \
  --filter-pattern "ERROR" \
  --start-time $(date -d '15 minutes ago' +%s000)
```

---

### 2.3 Check Metrics

**Key metrics to review:**
- Request rate
- Error rate (4xx, 5xx)
- Latency (p50, p95, p99)
- CPU/Memory usage
- Database connections
- Queue depths

---

### 2.4 Common Issues Decision Tree

```
API returning 5xx
├── Check pod status
│   ├── CrashLoopBackOff → Check logs for crash reason
│   ├── OOMKilled → Increase memory limits
│   └── Pending → Check node resources
├── Check database
│   ├── Connection refused → Check RDS status
│   ├── Query timeout → Check slow queries
│   └── Connection exhausted → Restart pods
├── Check external services
│   ├── Payment gateway down → Enable maintenance mode
│   └── Third-party API timeout → Check circuit breaker
└── Recent deployment?
    └── Yes → Consider rollback
```

---

## Section 3: Mitigation

### 3.1 Quick Mitigations

**Restart pods:**
```bash
kubectl rollout restart deployment/user-service -n amcart-prod
```

**Scale up:**
```bash
kubectl scale deployment/user-service --replicas=5 -n amcart-prod
```

**Rollback:**
```bash
kubectl rollout undo deployment/user-service -n amcart-prod
```

**Enable maintenance mode:**
```bash
# Update ConfigMap to enable maintenance
kubectl patch configmap amcart-config -n amcart-prod \
  -p '{"data":{"MAINTENANCE_MODE":"true"}}'
```

---

### 3.2 Service-Specific Mitigations

**User Service Down:**
```bash
# Check database connectivity
kubectl exec -it deployment/user-service -n amcart-prod -- \
  dotnet ef database health-check

# Restart service
kubectl rollout restart deployment/user-service -n amcart-prod
```

**Payment Service Down:**
```bash
# Check Razorpay status: https://status.razorpay.com
# Enable queue mode (if available)
kubectl patch deployment/payment-service -n amcart-prod \
  -p '{"spec":{"template":{"spec":{"containers":[{"name":"payment-service","env":[{"name":"QUEUE_MODE","value":"true"}]}]}}}}'
```

**Search Service Down:**
```bash
# Check OpenSearch cluster
curl -s https://amcart-opensearch.xxx.es.amazonaws.com/_cluster/health

# Fallback to database search (if implemented)
kubectl patch configmap search-config -n amcart-prod \
  -p '{"data":{"USE_FALLBACK":"true"}}'
```

**High Database Load:**
```bash
# Kill long-running queries
PGPASSWORD=$DB_PASSWORD psql -h amcart-prod.xxx.rds.amazonaws.com -U amcart -d amcart -c \
  "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE state = 'active' AND query_start < now() - interval '5 minutes';"

# Clear cache to reduce read pressure (carefully)
redis-cli -h amcart-redis.xxx.cache.amazonaws.com FLUSHDB
```

---

## Section 4: Communication

### 4.1 Status Updates (Every 15-30 minutes)

```
# In incident Slack channel:

⏱️ STATUS UPDATE (HH:MM UTC)
Impact: [Current customer impact]
Root Cause: [Known/Unknown/Suspected]
Mitigation: [Actions taken]
ETA: [Expected resolution time]
Next Update: [Time]
```

---

### 4.2 External Communication (P1/P2)

**Status Page Update:**
```
# Update status.amcart.com

Title: Service Degradation
Status: Investigating / Identified / Monitoring / Resolved
Message: We are experiencing issues with [service]. Our team is investigating.
Affected: [List affected components]
```

---

### 4.3 Escalation

| Condition | Escalate To |
|-----------|-------------|
| P1 not resolved in 30 min | Engineering Lead |
| Database issue | Database Admin |
| AWS infrastructure | DevOps Lead |
| Security incident | Security Team |
| Customer-facing > 1 hour | VP Engineering |

---

## Section 5: Resolution

### 5.1 Verify Resolution

```bash
# Check all services healthy
./scripts/health-check-all.sh

# Verify error rate back to normal
# Check Grafana dashboard

# Run smoke tests
./scripts/smoke-tests.sh production
```

---

### 5.2 Close Incident

```
# In incident Slack channel:

✅ INCIDENT RESOLVED
Duration: X hours Y minutes
Root Cause: [Brief description]
Resolution: [What fixed it]
Follow-up: [Ticket number for post-mortem]
```

---

### 5.3 Update Status Page

```
Title: Service Degradation - Resolved
Status: Resolved
Message: The issue has been resolved. All systems are operational.
```

---

## Section 6: Post-Incident

### 6.1 Post-Mortem (Required for P1/P2)

**Within 48 hours:**
1. Create post-mortem document
2. Gather timeline of events
3. Identify root cause
4. Define action items
5. Schedule review meeting

**Post-Mortem Template:**
```markdown
# Post-Mortem: [Incident Title]

## Summary
- Duration: X hours
- Impact: Y customers affected
- Severity: P1/P2

## Timeline
- HH:MM - Alert fired
- HH:MM - Investigation started
- HH:MM - Root cause identified
- HH:MM - Mitigation applied
- HH:MM - Resolved

## Root Cause
[Detailed explanation]

## Resolution
[What fixed the issue]

## Action Items
- [ ] [Action 1] - Owner - Due Date
- [ ] [Action 2] - Owner - Due Date

## Lessons Learned
- What went well
- What could be improved
```

---

### 6.2 Track Action Items

```bash
# Create tickets for each action item
# Link to post-mortem document
# Assign owners and due dates
```

---

## Common Scenarios

### Scenario: Complete Outage

1. ✅ Acknowledge alert
2. ✅ Check API Gateway / Load Balancer
3. ✅ Check Kubernetes nodes
4. ✅ Check database connectivity
5. ✅ Check DNS
6. ✅ Rollback recent deployments
7. ✅ Escalate if not resolved in 15 min

### Scenario: Payment Failures

1. ✅ Check Razorpay status page
2. ✅ Check Payment Service logs
3. ✅ Verify webhook connectivity
4. ✅ Check SSL certificates
5. ✅ Enable maintenance mode for checkout
6. ✅ Notify customer support

### Scenario: Slow Performance

1. ✅ Check CPU/Memory metrics
2. ✅ Check database slow queries
3. ✅ Check Redis memory
4. ✅ Check external API latency
5. ✅ Scale up if resource constrained
6. ✅ Clear cache if stale

---

## Related Runbooks

- [01-Service-Health-Check](01-Service-Health-Check.md)
- [02-Database-Operations](02-Database-Operations.md)
- [03-Deployment-Procedures](03-Deployment-Procedures.md)
- [07-Log-Analysis](07-Log-Analysis.md)

