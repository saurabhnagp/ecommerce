# Runbook: Log Analysis

## Purpose
Procedures for analyzing logs to debug issues and understand system behavior.

## When to Use
- Debugging production issues
- Investigating errors
- Understanding user behavior
- Performance analysis
- Security incident investigation

## Prerequisites
- kubectl access
- CloudWatch Logs access
- Kibana access (if EFK stack deployed)

---

## Section 1: Kubernetes Logs (kubectl)

### 1.1 View Pod Logs

```bash
# Current logs
kubectl logs deployment/user-service -n amcart-prod

# Follow logs in real-time
kubectl logs -f deployment/user-service -n amcart-prod

# Last 100 lines
kubectl logs deployment/user-service -n amcart-prod --tail=100

# Logs since timestamp
kubectl logs deployment/user-service -n amcart-prod --since=1h

# Logs from specific time
kubectl logs deployment/user-service -n amcart-prod --since-time=2024-12-19T10:00:00Z
```

---

### 1.2 View Logs from Previous Container

```bash
# Previous container (useful for crash debugging)
kubectl logs deployment/user-service -n amcart-prod --previous

# All containers in pod
kubectl logs pod/user-service-abc123 -n amcart-prod --all-containers
```

---

### 1.3 Filter Logs

```bash
# Search for errors
kubectl logs deployment/user-service -n amcart-prod | grep -i error

# Search for specific request ID
kubectl logs deployment/user-service -n amcart-prod | grep "request-id-123"

# Search for specific user
kubectl logs deployment/user-service -n amcart-prod | grep "user@example.com"

# Exclude health checks
kubectl logs deployment/user-service -n amcart-prod | grep -v "/health"
```

---

### 1.4 Logs from Multiple Pods

```bash
# Using label selector
kubectl logs -n amcart-prod -l app=user-service --tail=50

# Using stern (if installed)
stern user-service -n amcart-prod

# Stream from all services
stern -n amcart-prod --all-namespaces
```

---

## Section 2: CloudWatch Logs

### 2.1 Search Logs

```bash
# Filter log events
aws logs filter-log-events \
  --log-group-name /amcart/prod/user-service \
  --filter-pattern "ERROR" \
  --start-time $(date -d '1 hour ago' +%s000) \
  --limit 100

# Search for specific text
aws logs filter-log-events \
  --log-group-name /amcart/prod/user-service \
  --filter-pattern '"status":500' \
  --start-time $(date -d '1 hour ago' +%s000)
```

---

### 2.2 CloudWatch Logs Insights

```sql
-- Open CloudWatch Logs Insights in AWS Console
-- Select log group: /amcart/prod/user-service

-- Find errors in last hour
fields @timestamp, @message
| filter @message like /ERROR/
| sort @timestamp desc
| limit 100

-- Count errors by type
fields @timestamp, @message
| filter @message like /ERROR/
| parse @message /ERROR.*?(?<error_type>\w+Exception)/
| stats count() by error_type

-- Find slow requests (>1s)
fields @timestamp, @message
| parse @message /duration=(?<duration>\d+)ms/
| filter duration > 1000
| sort duration desc
| limit 50

-- Find requests by user
fields @timestamp, @message
| filter @message like /user@example.com/
| sort @timestamp desc
| limit 50

-- Error rate over time
fields @timestamp, @message
| filter @message like /ERROR/
| stats count() as errors by bin(5m)

-- Top error messages
fields @timestamp, @message
| filter @message like /ERROR/
| stats count() as count by @message
| sort count desc
| limit 20
```

---

### 2.3 Export Logs

```bash
# Create export task
aws logs create-export-task \
  --log-group-name /amcart/prod/user-service \
  --from $(date -d '1 day ago' +%s000) \
  --to $(date +%s000) \
  --destination amcart-log-exports \
  --destination-prefix user-service/$(date +%Y-%m-%d)

# Check export status
aws logs describe-export-tasks --status-code COMPLETED
```

---

## Section 3: Log Analysis Patterns

### 3.1 Identify Error Patterns

```bash
# Count errors by type
kubectl logs deployment/user-service -n amcart-prod --since=1h | \
  grep -i error | \
  sed 's/.*ERROR/ERROR/' | \
  sort | uniq -c | sort -rn | head -20

# Find error spikes
kubectl logs deployment/user-service -n amcart-prod --since=1h | \
  grep -i error | \
  cut -d' ' -f1 | \
  cut -d':' -f1-2 | \
  uniq -c
```

---

### 3.2 Trace Request Flow

```bash
# Given a request ID, find all related logs
REQUEST_ID="abc123"

# Search across all services
for svc in user-service product-service cart-service order-service; do
  echo "=== $svc ==="
  kubectl logs deployment/$svc -n amcart-prod | grep "$REQUEST_ID"
done
```

---

### 3.3 Analyze Response Times

```bash
# Extract response times from logs
kubectl logs deployment/user-service -n amcart-prod --since=1h | \
  grep "duration=" | \
  sed 's/.*duration=\([0-9]*\)ms.*/\1/' | \
  awk '{sum+=$1; count++} END {print "Avg:", sum/count, "ms"; print "Count:", count}'
```

---

## Section 4: Common Log Searches

### 4.1 Authentication Issues

```bash
# Failed logins
kubectl logs deployment/user-service -n amcart-prod | grep -i "authentication failed\|invalid credentials"

# Unauthorized access attempts
kubectl logs deployment/user-service -n amcart-prod | grep -i "unauthorized\|forbidden\|401\|403"

# Token issues
kubectl logs deployment/user-service -n amcart-prod | grep -i "token\|jwt" | grep -i "invalid\|expired"
```

---

### 4.2 Database Issues

```bash
# Connection errors
kubectl logs deployment/user-service -n amcart-prod | grep -i "connection\|timeout\|refused"

# Query errors
kubectl logs deployment/user-service -n amcart-prod | grep -i "sql\|query" | grep -i "error\|failed"

# Transaction issues
kubectl logs deployment/order-service -n amcart-prod | grep -i "transaction\|deadlock\|rollback"
```

---

### 4.3 External Service Issues

```bash
# Payment gateway
kubectl logs deployment/payment-service -n amcart-prod | grep -i "razorpay" | grep -i "error\|failed\|timeout"

# Email service
kubectl logs deployment/notification-service -n amcart-prod | grep -i "email\|smtp" | grep -i "error\|failed"

# Third-party APIs
kubectl logs deployment/product-service -n amcart-prod | grep -i "http\|api" | grep -i "5[0-9][0-9]\|timeout"
```

---

### 4.4 Performance Issues

```bash
# Slow requests
kubectl logs deployment/product-service -n amcart-prod | grep "duration=" | awk -F'duration=' '{print $2}' | awk -F'ms' '$1 > 1000 {print}'

# Memory warnings
kubectl logs deployment/user-service -n amcart-prod | grep -i "memory\|heap\|gc"

# High load
kubectl logs deployment/user-service -n amcart-prod | grep -i "queue\|backlog\|overloaded"
```

---

## Section 5: Structured Log Analysis

### 5.1 JSON Log Parsing

```bash
# Parse JSON logs with jq
kubectl logs deployment/user-service -n amcart-prod | \
  jq -r 'select(.level == "error") | "\(.timestamp) \(.message)"'

# Extract specific fields
kubectl logs deployment/user-service -n amcart-prod | \
  jq -r 'select(.path != null) | "\(.method) \(.path) \(.status_code) \(.duration_ms)ms"'

# Count by status code
kubectl logs deployment/user-service -n amcart-prod | \
  jq -r '.status_code' | sort | uniq -c | sort -rn
```

---

### 5.2 Serilog Format

```bash
# AmCart uses Serilog with JSON format
# Example log:
# {"Timestamp":"2024-12-19T10:00:00Z","Level":"Information","Message":"Request completed","Properties":{"Method":"GET","Path":"/api/users","StatusCode":200,"Duration":45}}

# Filter errors
kubectl logs deployment/user-service -n amcart-prod | \
  jq 'select(.Level == "Error")'

# Find slow requests
kubectl logs deployment/user-service -n amcart-prod | \
  jq 'select(.Properties.Duration > 1000)'
```

---

## Section 6: Log Aggregation (EFK)

### 6.1 Access Kibana

1. Port forward to Kibana
   ```bash
   kubectl port-forward svc/kibana 5601:5601 -n logging
   ```
2. Open http://localhost:5601
3. Navigate to Discover
4. Select `amcart-*` index pattern

---

### 6.2 Kibana Queries

```
# Errors in user-service
kubernetes.labels.app:"user-service" AND level:"error"

# 500 errors in last hour
status_code:500 AND @timestamp:[now-1h TO now]

# Specific user's requests
user_email:"customer@example.com"

# Slow requests
duration_ms:>1000

# Payment failures
service:"payment-service" AND (message:"failed" OR message:"error")
```

---

## Section 7: Troubleshooting Tips

### Quick Debug Commands

```bash
# Last error in each service
for svc in user-service product-service cart-service order-service payment-service; do
  echo "=== $svc ==="
  kubectl logs deployment/$svc -n amcart-prod --tail=1000 | grep -i error | tail -5
done

# Check for OOM kills
kubectl get pods -n amcart-prod -o json | jq '.items[] | select(.status.containerStatuses[].lastState.terminated.reason == "OOMKilled") | .metadata.name'

# Check restart counts
kubectl get pods -n amcart-prod -o custom-columns=NAME:.metadata.name,RESTARTS:.status.containerStatuses[0].restartCount | sort -k2 -rn

# Events in namespace
kubectl get events -n amcart-prod --sort-by='.lastTimestamp' | tail -20
```

---

## Log Retention

| Log Type | Location | Retention |
|----------|----------|-----------|
| Application | CloudWatch | 30 days |
| Kubernetes | CloudWatch | 14 days |
| Audit | CloudWatch | 365 days |
| Security | S3 | 7 years |

---

## Related Runbooks

- [01-Service-Health-Check](01-Service-Health-Check.md)
- [04-Incident-Response](04-Incident-Response.md)
- [08-Cache-Operations](08-Cache-Operations.md)

