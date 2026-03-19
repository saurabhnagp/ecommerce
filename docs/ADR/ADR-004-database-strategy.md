# ADR-004: Polyglot Persistence Strategy

## Status
**Accepted**

## Date
2024-12-19

## Context

AmCart has diverse data requirements:

| Data Type | Characteristics | Requirements |
|-----------|-----------------|--------------|
| Users, Orders, Payments | Relational, transactional | ACID, complex queries |
| Reviews, Notifications | Document-based, flexible | Schema flexibility |
| Sessions, Cart | Temporary, fast access | Low latency, TTL |
| Product Search | Full-text, faceted | Relevance scoring |

A single database cannot optimally serve all these needs.

## Decision

We will adopt a **polyglot persistence** strategy with four database types:

| Database | AWS Service | Purpose |
|----------|-------------|---------|
| PostgreSQL 15 | Amazon RDS | Transactional data |
| MongoDB 7.0 | Amazon DocumentDB | Document data |
| Redis 7.x | Amazon ElastiCache | Caching, sessions |
| Elasticsearch 8.x | Amazon OpenSearch | Search engine |

### Data Distribution

```
┌─────────────────────────────────────────────────────────────┐
│                    PostgreSQL (RDS)                          │
│  Users, Addresses, Products, Categories, Brands              │
│  Orders, Order Items, Payments, Inventory, Coupons           │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────┼─────────────────────────────┐
│                             │                              │
▼                             ▼                              ▼
┌─────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  MongoDB    │     │     Redis       │     │   OpenSearch    │
│ (DocumentDB)│     │  (ElastiCache)  │     │                 │
├─────────────┤     ├─────────────────┤     ├─────────────────┤
│ • Reviews   │     │ • Sessions      │     │ • Product Index │
│ • Notifs    │     │ • Cart          │     │ • Autocomplete  │
│ • Audit Logs│     │ • Cache         │     │ • Facets        │
│ • Activity  │     │ • Rate Limits   │     │ • Analytics     │
└─────────────┘     └─────────────────┘     └─────────────────┘
```

## Consequences

### Positive

- **Optimal Performance**: Each database optimized for its use case
- **Flexibility**: Choose best tool for each data type
- **Scalability**: Scale databases independently
- **Feature Set**: Full use of each database's capabilities

### Negative

- **Operational Complexity**: Multiple databases to manage
- **Data Synchronization**: Need to keep data in sync
- **Consistency Challenges**: Distributed transactions complex
- **Team Knowledge**: Need expertise in multiple databases
- **Cost**: Multiple managed services

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Complexity | AWS managed services reduce operations |
| Sync | Event-driven updates, CDC patterns |
| Consistency | Accept eventual consistency where appropriate |
| Knowledge | Training, documentation |
| Cost | Right-size instances, reserved capacity |

## Service-to-Database Mapping

| Service | Primary DB | Secondary | Cache |
|---------|------------|-----------|-------|
| User Service | PostgreSQL | - | Redis |
| Product Service | PostgreSQL | OpenSearch | Redis |
| Cart Service | Redis | PostgreSQL | - |
| Order Service | PostgreSQL | - | Redis |
| Payment Service | PostgreSQL | - | - |
| Inventory Service | PostgreSQL | - | Redis |
| Search Service | OpenSearch | - | Redis |
| Notification Service | MongoDB | - | Redis |
| Review Service | MongoDB | - | Redis |

## Alternatives Considered

### 1. Single PostgreSQL Database

**Pros:**
- Simpler operations
- ACID everywhere
- Single source of truth

**Cons:**
- JSON queries less efficient than MongoDB
- No native full-text search quality
- Cache would still be needed

**Why Rejected:** Cannot meet performance requirements for search and caching.

### 2. All-in-One Database (MongoDB or CockroachDB)

**Pros:**
- Single database to manage
- Flexible schema

**Cons:**
- Compromise on each use case
- Not optimal for relational data or search

**Why Rejected:** Too many compromises for enterprise ecommerce.

## Data Synchronization Strategy

### PostgreSQL → OpenSearch (Product Sync)

```
Product Service
     │
     ▼ (1) Update PostgreSQL
PostgreSQL
     │
     ▼ (2) Publish ProductUpdated event
RabbitMQ
     │
     ▼ (3) Search Service consumes
Search Service
     │
     ▼ (4) Update OpenSearch index
OpenSearch
```

### Redis Cache Invalidation

```
Product Service
     │
     ▼ (1) Update PostgreSQL
PostgreSQL
     │
     ▼ (2) Delete cache key
Redis
     │
     ▼ (3) Next read repopulates cache
```

## References

- [Polyglot Persistence](https://martinfowler.com/bliki/PolyglotPersistence.html)
- [AWS Database Selection Guide](https://aws.amazon.com/products/databases/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [Redis Documentation](https://redis.io/documentation)
- [Elasticsearch Documentation](https://www.elastic.co/guide/)

