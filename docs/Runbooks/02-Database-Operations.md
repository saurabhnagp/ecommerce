# Runbook: Database Operations

## Purpose
Procedures for database backup, restore, maintenance, and emergency operations.

## When to Use
- Scheduled maintenance
- Data recovery
- Performance issues
- Emergency database operations

## Prerequisites
- AWS RDS/DocumentDB/ElastiCache access
- DBA or elevated permissions
- Off-peak hours for maintenance

---

## Section 1: PostgreSQL (RDS) Operations

### 1.1 Manual Backup

```bash
# Create manual snapshot via AWS CLI
aws rds create-db-snapshot \
  --db-instance-identifier amcart-prod-postgres \
  --db-snapshot-identifier amcart-manual-$(date +%Y%m%d-%H%M%S)

# Verify snapshot status
aws rds describe-db-snapshots \
  --db-snapshot-identifier amcart-manual-YYYYMMDD-HHMMSS
```

**Expected:** Status = "available" (may take 5-30 minutes)

---

### 1.2 Restore from Snapshot

⚠️ **WARNING: This creates a NEW database instance**

```bash
# List available snapshots
aws rds describe-db-snapshots \
  --db-instance-identifier amcart-prod-postgres \
  --query 'DBSnapshots[*].[DBSnapshotIdentifier,SnapshotCreateTime,Status]'

# Restore to new instance
aws rds restore-db-instance-from-db-snapshot \
  --db-instance-identifier amcart-restored-postgres \
  --db-snapshot-identifier amcart-manual-YYYYMMDD-HHMMSS \
  --db-instance-class db.t3.medium \
  --vpc-security-group-ids sg-xxxxxxxxx \
  --db-subnet-group-name amcart-db-subnet
```

**Post-restore steps:**
1. Verify data integrity
2. Update application connection strings (if replacing primary)
3. Delete old instance after verification

---

### 1.3 Point-in-Time Recovery

```bash
# Restore to specific time (within retention period)
aws rds restore-db-instance-to-point-in-time \
  --source-db-instance-identifier amcart-prod-postgres \
  --target-db-instance-identifier amcart-pitr-postgres \
  --restore-time 2024-12-19T10:30:00Z \
  --db-instance-class db.t3.medium
```

---

### 1.4 Check Database Performance

```bash
# Connect to database
PGPASSWORD=$DB_PASSWORD psql -h amcart-prod.xxx.rds.amazonaws.com -U amcart -d amcart

# Check active queries
SELECT pid, now() - pg_stat_activity.query_start AS duration, query, state
FROM pg_stat_activity
WHERE (now() - pg_stat_activity.query_start) > interval '5 minutes';

# Check table sizes
SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename))
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC
LIMIT 10;

# Check index usage
SELECT indexrelname, idx_scan, idx_tup_read
FROM pg_stat_user_indexes
ORDER BY idx_scan ASC
LIMIT 20;
```

---

### 1.5 Kill Long-Running Query

```bash
# Find the query
SELECT pid, query FROM pg_stat_activity WHERE state = 'active' AND duration > '5 min';

# Cancel query (graceful)
SELECT pg_cancel_backend(<pid>);

# Terminate connection (force)
SELECT pg_terminate_backend(<pid>);
```

---

## Section 2: Redis (ElastiCache) Operations

### 2.1 Check Redis Memory

```bash
# Connect via pod
kubectl exec -it deployment/cart-service -n amcart-prod -- \
  redis-cli -h amcart-redis.xxx.cache.amazonaws.com INFO memory

# Key metrics:
# - used_memory_human
# - used_memory_peak_human
# - maxmemory_human
```

---

### 2.2 Clear Specific Cache

```bash
# Connect to Redis
kubectl exec -it deployment/cart-service -n amcart-prod -- \
  redis-cli -h amcart-redis.xxx.cache.amazonaws.com

# Delete by pattern (CAUTION)
SCAN 0 MATCH "product:*" COUNT 100
# For each key found:
DEL <key>

# Or using redis-cli
redis-cli -h amcart-redis.xxx.cache.amazonaws.com --scan --pattern "product:*" | xargs redis-cli DEL
```

---

### 2.3 Flush All Cache

⚠️ **WARNING: This will clear ALL cached data including sessions**

```bash
# During maintenance window ONLY
redis-cli -h amcart-redis.xxx.cache.amazonaws.com FLUSHALL

# Alternative: Flush specific database
redis-cli -h amcart-redis.xxx.cache.amazonaws.com -n 0 FLUSHDB
```

**Post-flush:**
- Users may need to re-login
- Cache will warm up gradually
- Monitor for increased database load

---

### 2.4 Redis Snapshot

```bash
# Create snapshot
aws elasticache create-snapshot \
  --replication-group-id amcart-redis \
  --snapshot-name amcart-redis-$(date +%Y%m%d)

# List snapshots
aws elasticache describe-snapshots \
  --replication-group-id amcart-redis
```

---

## Section 3: MongoDB (DocumentDB) Operations

### 3.1 Manual Backup

```bash
# Create cluster snapshot
aws docdb create-db-cluster-snapshot \
  --db-cluster-identifier amcart-docdb \
  --db-cluster-snapshot-identifier amcart-docdb-$(date +%Y%m%d)

# Check snapshot status
aws docdb describe-db-cluster-snapshots \
  --db-cluster-snapshot-identifier amcart-docdb-YYYYMMDD
```

---

### 3.2 Check Collection Stats

```javascript
// Connect via mongosh
use reviews

// Collection stats
db.reviews.stats()

// Index stats
db.reviews.aggregate([{$indexStats: {}}])

// Slow queries
db.setProfilingLevel(1, 100) // Log queries > 100ms
db.system.profile.find().sort({ts: -1}).limit(10)
```

---

### 3.3 Create Index

```javascript
// Connect to DocumentDB
use reviews

// Create index (background)
db.reviews.createIndex(
  { "productId": 1, "createdAt": -1 },
  { background: true, name: "idx_product_date" }
)

// Verify index
db.reviews.getIndexes()
```

---

## Section 4: OpenSearch Operations

### 4.1 Check Cluster Health

```bash
# Cluster health
curl -s https://amcart-opensearch.xxx.es.amazonaws.com/_cluster/health | jq

# Node stats
curl -s https://amcart-opensearch.xxx.es.amazonaws.com/_nodes/stats | jq '.nodes | to_entries | .[].value | {name, heap_used_percent: .jvm.mem.heap_used_percent}'
```

---

### 4.2 Reindex Products

```bash
# Create new index
curl -X PUT "https://amcart-opensearch.xxx.es.amazonaws.com/products-v2" \
  -H "Content-Type: application/json" \
  -d @products-mapping.json

# Reindex from old to new
curl -X POST "https://amcart-opensearch.xxx.es.amazonaws.com/_reindex" \
  -H "Content-Type: application/json" \
  -d '{
    "source": {"index": "products-v1"},
    "dest": {"index": "products-v2"}
  }'

# Switch alias
curl -X POST "https://amcart-opensearch.xxx.es.amazonaws.com/_aliases" \
  -H "Content-Type: application/json" \
  -d '{
    "actions": [
      {"remove": {"index": "products-v1", "alias": "products"}},
      {"add": {"index": "products-v2", "alias": "products"}}
    ]
  }'
```

---

### 4.3 Delete Old Indices

```bash
# List indices
curl -s https://amcart-opensearch.xxx.es.amazonaws.com/_cat/indices?v

# Delete index (CAUTION)
curl -X DELETE "https://amcart-opensearch.xxx.es.amazonaws.com/products-v1"
```

---

## Section 5: Emergency Procedures

### 5.1 Database Connection Issues

1. Check security groups
2. Check VPC routing
3. Verify credentials in Secrets Manager
4. Check RDS/DocumentDB instance status
5. Review CloudWatch metrics for CPU/memory

### 5.2 Database Full

```bash
# Check storage
aws rds describe-db-instances \
  --db-instance-identifier amcart-prod-postgres \
  --query 'DBInstances[*].[AllocatedStorage,FreeStorageSpace]'

# Increase storage (online)
aws rds modify-db-instance \
  --db-instance-identifier amcart-prod-postgres \
  --allocated-storage 200 \
  --apply-immediately
```

### 5.3 Connection Pool Exhausted

```sql
-- Check active connections
SELECT count(*) FROM pg_stat_activity;

-- Check max connections
SHOW max_connections;

-- Kill idle connections
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE state = 'idle'
  AND query_start < now() - interval '30 minutes';
```

---

## Maintenance Schedule

| Database | Maintenance | Window | Frequency |
|----------|-------------|--------|-----------|
| PostgreSQL | Auto backup | 03:00-04:00 UTC | Daily |
| PostgreSQL | Vacuum | 02:00-03:00 UTC | Weekly (Sun) |
| Redis | Snapshot | 03:00-03:30 UTC | Daily |
| DocumentDB | Auto backup | 03:00-04:00 UTC | Daily |
| OpenSearch | Index rotation | 04:00-05:00 UTC | Monthly |

---

## Related Runbooks

- [01-Service-Health-Check](01-Service-Health-Check.md)
- [04-Incident-Response](04-Incident-Response.md)
- [08-Cache-Operations](08-Cache-Operations.md)

