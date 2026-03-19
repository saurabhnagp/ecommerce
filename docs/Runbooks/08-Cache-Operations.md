# Runbook: Cache Operations

## Purpose
Procedures for managing Redis cache, troubleshooting cache issues, and cache maintenance.

## When to Use
- Cache performance issues
- Stale data investigation
- Cache capacity problems
- Cache flush requirements
- Debugging cache-related bugs

## Prerequisites
- Redis CLI access (via kubectl exec)
- AWS ElastiCache console access
- Understanding of cache key patterns

---

## Section 1: Connecting to Redis

### 1.1 Connect via kubectl

```bash
# Exec into a service pod
kubectl exec -it deployment/cart-service -n amcart-prod -- /bin/sh

# Connect to Redis
redis-cli -h amcart-redis.xxx.cache.amazonaws.com -p 6379

# With authentication (if enabled)
redis-cli -h amcart-redis.xxx.cache.amazonaws.com -p 6379 -a PASSWORD
```

---

### 1.2 One-liner Commands

```bash
# Execute single command
kubectl exec -it deployment/cart-service -n amcart-prod -- \
  redis-cli -h amcart-redis.xxx.cache.amazonaws.com PING

# Get specific key
kubectl exec -it deployment/cart-service -n amcart-prod -- \
  redis-cli -h amcart-redis.xxx.cache.amazonaws.com GET "product:detail:123"
```

---

## Section 2: Cache Health Monitoring

### 2.1 Check Redis Info

```bash
# Overall info
redis-cli INFO

# Memory info
redis-cli INFO memory

# Key metrics to check:
# - used_memory_human
# - used_memory_peak_human
# - maxmemory_human
# - evicted_keys (should be 0 or low)

# Stats
redis-cli INFO stats
# - keyspace_hits (cache hits)
# - keyspace_misses (cache misses)
# - Hit ratio = hits / (hits + misses)

# Replication
redis-cli INFO replication

# Clients
redis-cli INFO clients
# - connected_clients
# - blocked_clients (should be 0)
```

---

### 2.2 Check Key Statistics

```bash
# Total keys
redis-cli DBSIZE

# Keys by pattern (slow on large datasets!)
redis-cli --scan --pattern "product:*" | wc -l
redis-cli --scan --pattern "session:*" | wc -l
redis-cli --scan --pattern "cart:*" | wc -l

# Memory usage of key
redis-cli MEMORY USAGE "product:detail:123"

# Key TTL
redis-cli TTL "product:detail:123"
```

---

### 2.3 Check Slow Log

```bash
# Get slow queries (operations > 10ms by default)
redis-cli SLOWLOG GET 10

# Reset slow log
redis-cli SLOWLOG RESET

# Configure slow log threshold (microseconds)
redis-cli CONFIG SET slowlog-log-slower-than 10000
```

---

## Section 3: Cache Key Management

### 3.1 AmCart Cache Key Patterns

| Pattern | Purpose | TTL |
|---------|---------|-----|
| `product:detail:{id}` | Product details | 30 min |
| `product:list:{category}:{page}` | Product listings | 5 min |
| `cart:user:{userId}` | Shopping cart | 7 days |
| `session:{token}` | User session | 30 min |
| `refresh:{token}` | Refresh token | 7 days |
| `ratelimit:{ip}:{endpoint}` | Rate limiting | 1 min |
| `user:profile:{id}` | User profile | 15 min |
| `search:results:{hash}` | Search cache | 5 min |

---

### 3.2 View Keys

```bash
# List keys (NEVER use KEYS in production!)
# Use SCAN instead
redis-cli SCAN 0 MATCH "product:*" COUNT 100

# Get key type
redis-cli TYPE "cart:user:abc123"

# Get string value
redis-cli GET "product:detail:123"

# Get hash
redis-cli HGETALL "user:profile:abc123"

# Get list
redis-cli LRANGE "user:notifications:abc123" 0 -1
```

---

### 3.3 Delete Keys

```bash
# Delete single key
redis-cli DEL "product:detail:123"

# Delete by pattern (use with caution!)
redis-cli --scan --pattern "product:list:*" | xargs redis-cli DEL

# Safe batch delete
redis-cli --scan --pattern "product:list:*" | while read key; do
  redis-cli DEL "$key"
  sleep 0.01  # Rate limit
done
```

---

## Section 4: Cache Invalidation

### 4.1 Invalidate Product Cache

```bash
# Single product
redis-cli DEL "product:detail:123"

# All products in category
redis-cli --scan --pattern "product:list:category:electronics:*" | xargs redis-cli DEL

# All product cache
redis-cli --scan --pattern "product:*" | xargs redis-cli DEL
```

---

### 4.2 Invalidate User Cache

```bash
# Single user profile
redis-cli DEL "user:profile:abc123"

# User session (forces re-login)
redis-cli --scan --pattern "session:*:abc123" | xargs redis-cli DEL

# All sessions for user
redis-cli --scan --pattern "*:user:abc123" | xargs redis-cli DEL
```

---

### 4.3 Invalidate All Cache

⚠️ **WARNING: This will clear ALL cached data**

```bash
# Flush current database (default DB 0)
redis-cli FLUSHDB

# Flush all databases
redis-cli FLUSHALL

# Flush with async (non-blocking)
redis-cli FLUSHDB ASYNC
```

**After flush:**
- Users may need to re-login
- Expect increased database load
- Monitor for errors

---

## Section 5: Session Management

### 5.1 View Sessions

```bash
# Count active sessions
redis-cli --scan --pattern "session:*" | wc -l

# View session data
redis-cli GET "session:abc123token"

# Check session expiry
redis-cli TTL "session:abc123token"
```

---

### 5.2 Terminate Sessions

```bash
# Terminate specific session
redis-cli DEL "session:abc123token"

# Terminate all sessions (logs out all users)
redis-cli --scan --pattern "session:*" | xargs redis-cli DEL

# Terminate sessions for specific user
USER_ID="user-uuid"
redis-cli --scan --pattern "session:*" | while read key; do
  content=$(redis-cli GET "$key")
  if echo "$content" | grep -q "$USER_ID"; then
    redis-cli DEL "$key"
  fi
done
```

---

## Section 6: Shopping Cart Operations

### 6.1 View Cart

```bash
# Get cart for user
redis-cli GET "cart:user:abc123"

# Pretty print (if JSON)
redis-cli GET "cart:user:abc123" | jq
```

---

### 6.2 Cart Recovery

```bash
# Extend cart TTL (if about to expire)
redis-cli EXPIRE "cart:user:abc123" 604800  # 7 days

# Backup cart to file
redis-cli GET "cart:user:abc123" > cart_backup.json

# Restore cart
redis-cli SET "cart:user:abc123" "$(cat cart_backup.json)" EX 604800
```

---

### 6.3 Fix Orphaned Carts

```bash
# Find carts with no recent activity
redis-cli --scan --pattern "cart:user:*" | while read key; do
  ttl=$(redis-cli TTL "$key")
  if [ "$ttl" -lt 3600 ]; then
    echo "$key has TTL $ttl seconds"
  fi
done
```

---

## Section 7: Rate Limiting

### 7.1 Check Rate Limits

```bash
# View rate limit for IP
redis-cli GET "ratelimit:ip:1.2.3.4:/api/v1/auth/login"

# View all rate limits
redis-cli --scan --pattern "ratelimit:*" | head -20
```

---

### 7.2 Clear Rate Limits

```bash
# Clear for specific IP
redis-cli DEL "ratelimit:ip:1.2.3.4:/api/v1/auth/login"

# Clear all rate limits for IP
redis-cli --scan --pattern "ratelimit:ip:1.2.3.4:*" | xargs redis-cli DEL

# Clear all rate limits (use with caution)
redis-cli --scan --pattern "ratelimit:*" | xargs redis-cli DEL
```

---

## Section 8: Troubleshooting

### 8.1 High Memory Usage

```bash
# Check memory
redis-cli INFO memory

# Find large keys
redis-cli --bigkeys

# Memory analysis
redis-cli MEMORY DOCTOR

# If memory is critical:
# 1. Check maxmemory policy
redis-cli CONFIG GET maxmemory-policy

# 2. Set eviction policy if not set
redis-cli CONFIG SET maxmemory-policy allkeys-lru

# 3. Manually remove less important data
redis-cli --scan --pattern "search:*" | xargs redis-cli DEL
```

---

### 8.2 High Connection Count

```bash
# Check client connections
redis-cli CLIENT LIST

# Count connections
redis-cli CLIENT LIST | wc -l

# Kill idle connections (>600 seconds)
redis-cli CLIENT KILL TYPE normal IDLE 600
```

---

### 8.3 Slow Performance

```bash
# Check slow log
redis-cli SLOWLOG GET 25

# Check if blocking operations
redis-cli CLIENT LIST | grep "blocked"

# Check if commands taking too long
redis-cli INFO commandstats
```

---

### 8.4 Cache Miss Debugging

```bash
# Calculate hit rate
hits=$(redis-cli INFO stats | grep keyspace_hits | cut -d: -f2 | tr -d '\r')
misses=$(redis-cli INFO stats | grep keyspace_misses | cut -d: -f2 | tr -d '\r')
ratio=$(echo "scale=2; $hits / ($hits + $misses) * 100" | bc)
echo "Cache hit rate: $ratio%"

# If hit rate is low:
# 1. Check if TTLs are too short
# 2. Check if keys are being invalidated too often
# 3. Check application logs for cache misses
```

---

## Section 9: Maintenance

### 9.1 Create Snapshot

```bash
# Via AWS CLI
aws elasticache create-snapshot \
  --replication-group-id amcart-redis \
  --snapshot-name amcart-redis-$(date +%Y%m%d-%H%M%S)

# List snapshots
aws elasticache describe-snapshots \
  --replication-group-id amcart-redis
```

---

### 9.2 Restore from Snapshot

```bash
# Create new cluster from snapshot
aws elasticache create-replication-group \
  --replication-group-id amcart-redis-restored \
  --replication-group-description "Restored from snapshot" \
  --snapshot-name amcart-redis-20241219-120000
```

---

### 9.3 Monitor ElastiCache

Key CloudWatch metrics:
- `CPUUtilization`
- `FreeableMemory`
- `CurrConnections`
- `Evictions`
- `CacheHits` / `CacheMisses`

---

## Quick Reference

| Task | Command |
|------|---------|
| Ping | `redis-cli PING` |
| Key count | `redis-cli DBSIZE` |
| Get key | `redis-cli GET key` |
| Delete key | `redis-cli DEL key` |
| Set TTL | `redis-cli EXPIRE key seconds` |
| Check TTL | `redis-cli TTL key` |
| Find keys | `redis-cli --scan --pattern "prefix:*"` |
| Memory info | `redis-cli INFO memory` |
| Flush DB | `redis-cli FLUSHDB` |
| Slow queries | `redis-cli SLOWLOG GET 10` |

---

## Related Runbooks

- [01-Service-Health-Check](01-Service-Health-Check.md)
- [02-Database-Operations](02-Database-Operations.md)
- [04-Incident-Response](04-Incident-Response.md)

