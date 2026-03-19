# Runbook: Scaling Procedures

## Purpose
Procedures for scaling services and infrastructure to handle traffic changes.

## When to Use
- Anticipated high traffic (sales events, marketing campaigns)
- Reactive scaling during traffic spikes
- Cost optimization (scaling down)
- Capacity planning

## Prerequisites
- kubectl access
- AWS Console access
- Approval for significant changes

---

## Section 1: Kubernetes Pod Scaling

### 1.1 Manual Pod Scaling

```bash
# Scale specific deployment
kubectl scale deployment/product-service --replicas=5 -n amcart-prod

# Scale multiple deployments
for svc in user-service product-service cart-service order-service; do
  kubectl scale deployment/$svc --replicas=5 -n amcart-prod
done

# Verify scaling
kubectl get pods -n amcart-prod -l app=product-service
```

---

### 1.2 Check Current HPA Status

```bash
# View all HPAs
kubectl get hpa -n amcart-prod

# Detailed HPA info
kubectl describe hpa product-service-hpa -n amcart-prod

# Example output:
# Name:                product-service-hpa
# Min replicas:        2
# Max replicas:        10
# Current replicas:    3
# Conditions:
#   AbleToScale: True
#   ScalingActive: True
```

---

### 1.3 Update HPA Limits

```bash
# Increase max replicas
kubectl patch hpa product-service-hpa -n amcart-prod \
  -p '{"spec":{"maxReplicas":20}}'

# Update scaling thresholds
kubectl patch hpa product-service-hpa -n amcart-prod \
  -p '{"spec":{"metrics":[{"type":"Resource","resource":{"name":"cpu","target":{"type":"Utilization","averageUtilization":60}}}]}}'

# Apply new HPA configuration
kubectl apply -f - <<EOF
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: product-service-hpa
  namespace: amcart-prod
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: product-service
  minReplicas: 3
  maxReplicas: 20
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 60
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 70
EOF
```

---

## Section 2: Kubernetes Node Scaling

### 2.1 Check Node Status

```bash
# View nodes
kubectl get nodes -o wide

# Check node resources
kubectl top nodes

# Check allocatable vs used
kubectl describe node <node-name> | grep -A 5 "Allocated resources"
```

---

### 2.2 Scale EKS Node Group (AWS Console)

1. Open AWS Console → EKS → Clusters → amcart-prod
2. Go to Compute tab → Node Groups
3. Select node group → Edit
4. Update Desired size, Minimum size, Maximum size
5. Save changes

---

### 2.3 Scale EKS Node Group (CLI)

```bash
# Update node group scaling
aws eks update-nodegroup-config \
  --cluster-name amcart-prod \
  --nodegroup-name amcart-prod-ng \
  --scaling-config minSize=3,maxSize=10,desiredSize=5

# Verify update
aws eks describe-nodegroup \
  --cluster-name amcart-prod \
  --nodegroup-name amcart-prod-ng \
  --query 'nodegroup.scalingConfig'
```

---

### 2.4 Enable Cluster Autoscaler

```bash
# Verify cluster autoscaler is running
kubectl get pods -n kube-system -l app.kubernetes.io/name=cluster-autoscaler

# Check autoscaler logs
kubectl logs -f deployment/cluster-autoscaler -n kube-system

# Verify autoscaler config
kubectl get configmap cluster-autoscaler-status -n kube-system -o yaml
```

---

## Section 3: Database Scaling

### 3.1 RDS Vertical Scaling

⚠️ **WARNING: This causes brief downtime**

```bash
# Check current instance class
aws rds describe-db-instances \
  --db-instance-identifier amcart-prod-postgres \
  --query 'DBInstances[0].DBInstanceClass'

# Scale up (during maintenance window)
aws rds modify-db-instance \
  --db-instance-identifier amcart-prod-postgres \
  --db-instance-class db.t3.large \
  --apply-immediately

# Monitor modification status
aws rds describe-db-instances \
  --db-instance-identifier amcart-prod-postgres \
  --query 'DBInstances[0].DBInstanceStatus'
```

---

### 3.2 RDS Read Replicas

```bash
# Create read replica
aws rds create-db-instance-read-replica \
  --db-instance-identifier amcart-prod-postgres-read \
  --source-db-instance-identifier amcart-prod-postgres \
  --db-instance-class db.t3.medium

# List replicas
aws rds describe-db-instances \
  --query 'DBInstances[?ReadReplicaSourceDBInstanceIdentifier!=`null`].[DBInstanceIdentifier,ReadReplicaSourceDBInstanceIdentifier]'
```

---

### 3.3 Redis Scaling

```bash
# Scale Redis cluster (add shards)
aws elasticache modify-replication-group \
  --replication-group-id amcart-redis \
  --node-group-count 3 \
  --apply-immediately

# Scale Redis node type
aws elasticache modify-replication-group \
  --replication-group-id amcart-redis \
  --cache-node-type cache.r6g.large \
  --apply-immediately
```

---

### 3.4 OpenSearch Scaling

```bash
# Update OpenSearch domain
aws opensearch update-domain-config \
  --domain-name amcart-search \
  --cluster-config InstanceType=m5.large.search,InstanceCount=3

# Monitor update status
aws opensearch describe-domain \
  --domain-name amcart-search \
  --query 'DomainStatus.Processing'
```

---

## Section 4: Pre-Event Scaling (Sales, Campaigns)

### 4.1 Pre-Event Checklist

- [ ] Estimate expected traffic increase
- [ ] Scale pods (increase min replicas)
- [ ] Scale nodes (increase node count)
- [ ] Scale databases (if needed)
- [ ] Scale Redis
- [ ] Warm up caches
- [ ] Notify team

---

### 4.2 Recommended Scale-Up Plan

| Traffic Increase | Pod Replicas | Node Count | RDS Class | Redis |
|-----------------|--------------|------------|-----------|-------|
| 2x normal | 1.5x | +1 node | Same | Same |
| 5x normal | 3x | +2 nodes | 1 size up | 1 size up |
| 10x normal | 5x | +5 nodes | 2 sizes up | Add shards |

---

### 4.3 Scale-Up Script

```bash
#!/bin/bash
# pre-event-scale.sh

echo "🚀 Pre-Event Scaling"
echo "===================="

# Scale pods
echo "Scaling pods..."
for svc in user-service product-service cart-service order-service payment-service search-service; do
  kubectl scale deployment/$svc --replicas=5 -n amcart-prod
  kubectl patch hpa ${svc}-hpa -n amcart-prod -p '{"spec":{"minReplicas":5,"maxReplicas":20}}'
done

# Scale nodes
echo "Scaling nodes..."
aws eks update-nodegroup-config \
  --cluster-name amcart-prod \
  --nodegroup-name amcart-prod-ng \
  --scaling-config minSize=5,maxSize=15,desiredSize=7

# Warm cache
echo "Warming cache..."
curl -s https://api.amcart.com/api/v1/products/warm-cache

echo "✅ Scale-up complete"
```

---

### 4.4 Scale-Down Script (Post-Event)

```bash
#!/bin/bash
# post-event-scale-down.sh

echo "📉 Post-Event Scale Down"
echo "========================"

# Reset HPA
for svc in user-service product-service cart-service order-service payment-service search-service; do
  kubectl patch hpa ${svc}-hpa -n amcart-prod -p '{"spec":{"minReplicas":2,"maxReplicas":10}}'
done

# Scale down nodes (let autoscaler handle pods)
aws eks update-nodegroup-config \
  --cluster-name amcart-prod \
  --nodegroup-name amcart-prod-ng \
  --scaling-config minSize=3,maxSize=10,desiredSize=3

echo "✅ Scale-down initiated"
echo "Note: Pods will scale down gradually based on HPA"
```

---

## Section 5: Monitoring During Scaling

### 5.1 Key Metrics to Watch

```bash
# Pod CPU/Memory
kubectl top pods -n amcart-prod

# Node resources
kubectl top nodes

# Pending pods (indicates need for more nodes)
kubectl get pods -n amcart-prod | grep Pending
```

---

### 5.2 CloudWatch Metrics

Monitor these metrics in CloudWatch:
- `AWS/EKS` → CPUUtilization
- `AWS/RDS` → CPUUtilization, DatabaseConnections
- `AWS/ElastiCache` → CPUUtilization, CurrConnections
- `AWS/ApplicationELB` → RequestCount, TargetResponseTime

---

## Section 6: Cost-Aware Scaling

### 6.1 Reserved Instances

- Use Reserved Instances for baseline capacity
- Use On-Demand for scaling headroom
- Consider Spot Instances for non-critical workloads

### 6.2 Scheduled Scaling

```bash
# Scale down overnight (example: cron job)
# 0 2 * * * /scripts/scale-down-overnight.sh

# Scale up for business hours
# 0 7 * * * /scripts/scale-up-business-hours.sh
```

---

## Related Runbooks

- [01-Service-Health-Check](01-Service-Health-Check.md)
- [02-Database-Operations](02-Database-Operations.md)
- [04-Incident-Response](04-Incident-Response.md)

