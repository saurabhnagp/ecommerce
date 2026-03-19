# DECISION ANALYSIS AND RESOLUTION (DAR) DOCUMENT

## Database Technology Selection - Polyglot Persistence Architecture

---

## Document Control Information

| Field | Value |
|:------|:------|
| **Document Title** | Database Technology Selection - Polyglot Persistence |
| **Document ID** | DAR-AMCART-DB-001 |
| **Project Name** | AmCart Ecommerce Platform |
| **Version** | 1.0 |
| **Status** | Approved |
| **Created Date** | December 19, 2024 |
| **Last Updated** | December 19, 2024 |
| **Prepared By** | Technical Architecture Team |
| **Reviewed By** | [Reviewer Name] |
| **Approved By** | [Approver Name] |

---

## 1. Executive Summary

### 1.1 Purpose

This Decision Analysis and Resolution (DAR) document records the evaluation and selection of database technologies for the AmCart Ecommerce Platform using a **polyglot persistence** approach. Each database is selected based on the specific data access patterns and requirements of different microservices.

### 1.2 Decision Statement

After comprehensive evaluation of database technologies against project requirements, the following databases have been selected:

| Database Type | Selected Technology | AWS Service |
|:--------------|:--------------------|:------------|
| **Relational (OLTP)** | PostgreSQL 15 | Amazon RDS |
| **Document Store** | MongoDB 7.0 | Amazon DocumentDB |
| **In-Memory Cache** | Redis 7.x | Amazon ElastiCache |
| **Search Engine** | Elasticsearch 8.x | Amazon OpenSearch |

### 1.3 Polyglot Persistence Strategy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Polyglot Persistence Architecture                        │
│                                                                              │
│  "Use the right database for the right job"                                 │
│                                                                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐             │
│  │   PostgreSQL    │  │    MongoDB      │  │     Redis       │             │
│  │   (RDS)         │  │  (DocumentDB)   │  │  (ElastiCache)  │             │
│  ├─────────────────┤  ├─────────────────┤  ├─────────────────┤             │
│  │ • Users         │  │ • Reviews       │  │ • Sessions      │             │
│  │ • Orders        │  │ • Notifications │  │ • Cart          │             │
│  │ • Payments      │  │ • Audit Logs    │  │ • Cache         │             │
│  │ • Products      │  │ • Activity      │  │ • Rate Limits   │             │
│  │ • Inventory     │  │ • CMS Content   │  │ • Pub/Sub       │             │
│  │ • Categories    │  │                 │  │                 │             │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘             │
│           │                    │                    │                       │
│           └────────────────────┴────────────────────┘                       │
│                                │                                            │
│                    ┌───────────┴───────────┐                                │
│                    │    OpenSearch         │                                │
│                    │  (Search Engine)      │                                │
│                    ├───────────────────────┤                                │
│                    │ • Product Search      │                                │
│                    │ • Autocomplete        │                                │
│                    │ • Faceted Filters     │                                │
│                    │ • Analytics           │                                │
│                    └───────────────────────┘                                │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Business Context

### 2.1 Project Background

AmCart is an ecommerce platform requiring multiple database technologies to handle diverse data patterns including:
- Transactional data with ACID compliance (orders, payments)
- Flexible schema documents (reviews, notifications)
- High-speed caching and session management
- Full-text search with relevance scoring

### 2.2 Data Requirements

| Requirement | Priority | Database Need |
|:------------|:---------|:--------------|
| ACID transactions for orders/payments | Critical | Relational DB |
| Flexible schema for reviews/notifications | High | Document DB |
| Sub-millisecond cache access | High | In-Memory DB |
| Full-text product search | High | Search Engine |
| User session management | High | In-Memory DB |
| Shopping cart persistence | High | In-Memory + Backup |
| Audit logging | Medium | Document DB |
| Real-time inventory | High | In-Memory DB |

### 2.3 Non-Functional Requirements

| Requirement | Target | Impact on DB Selection |
|:------------|:-------|:-----------------------|
| Availability | 99.9% | Multi-AZ deployment |
| Read Latency | < 50ms | Caching layer required |
| Write Latency | < 100ms | Optimized writes |
| Data Durability | 99.999999999% | Replication, backups |
| Scalability | 10x growth | Horizontal scaling capable |
| Disaster Recovery | RPO < 1hr, RTO < 4hr | Cross-region replication |

---

## 3. Database Comparison Matrix

### 3.1 Relational Databases (SQL)

| Feature | PostgreSQL | MySQL | SQL Server | Aurora |
|:--------|:-----------|:------|:-----------|:-------|
| **ACID Compliance** | ✅ Full | ✅ Full | ✅ Full | ✅ Full |
| **JSON Support** | ✅ Excellent | ⚠️ Limited | ⚠️ Limited | ✅ Good |
| **Full-Text Search** | ✅ Built-in | ✅ Built-in | ✅ Built-in | ✅ Built-in |
| **Partitioning** | ✅ Native | ✅ Native | ✅ Enterprise | ✅ Native |
| **Replication** | ✅ Streaming | ✅ Multiple | ✅ Always On | ✅ Built-in |
| **AWS Integration** | ✅ RDS | ✅ RDS | ✅ RDS | ✅ Native |
| **License Cost** | Free | Free | $$$$ | $$ |
| **Community** | Excellent | Excellent | Good | Good |
| **.NET Support** | ✅ Npgsql | ✅ MySqlConnector | ✅ Native | ✅ Npgsql |
| **Overall Rating** | **9.5/10** | 8.5/10 | 8.0/10 | 9.0/10 |

### 3.2 Document Databases (NoSQL)

| Feature | MongoDB | DocumentDB | DynamoDB | CouchDB |
|:--------|:--------|:-----------|:---------|:--------|
| **Schema Flexibility** | ✅ Excellent | ✅ Excellent | ✅ Good | ✅ Excellent |
| **Query Language** | ✅ Rich | ✅ MongoDB-compatible | ⚠️ Limited | ⚠️ Limited |
| **Aggregation** | ✅ Pipeline | ✅ Pipeline | ⚠️ Limited | ⚠️ MapReduce |
| **Transactions** | ✅ Multi-doc | ✅ Multi-doc | ✅ Limited | ❌ No |
| **Horizontal Scaling** | ✅ Sharding | ✅ Auto | ✅ Auto | ✅ Cluster |
| **AWS Managed** | ❌ Self-host | ✅ Native | ✅ Native | ❌ Self-host |
| **MongoDB Driver** | ✅ Native | ✅ Compatible | ❌ Different | ❌ Different |
| **.NET Support** | ✅ Official | ✅ Compatible | ✅ SDK | ⚠️ Community |
| **Overall Rating** | 9.0/10 | **9.2/10** | 8.5/10 | 7.0/10 |

### 3.3 In-Memory Databases

| Feature | Redis | Memcached | AWS DAX | Hazelcast |
|:--------|:------|:----------|:--------|:----------|
| **Data Structures** | ✅ Rich | ❌ Key-Value only | ❌ DynamoDB only | ✅ Rich |
| **Persistence** | ✅ RDB/AOF | ❌ No | N/A | ✅ Yes |
| **Pub/Sub** | ✅ Built-in | ❌ No | ❌ No | ✅ Yes |
| **Clustering** | ✅ Native | ✅ Yes | ✅ Yes | ✅ Yes |
| **TTL Support** | ✅ Per-key | ✅ Per-key | ✅ Yes | ✅ Yes |
| **Lua Scripting** | ✅ Yes | ❌ No | ❌ No | ⚠️ Limited |
| **AWS Managed** | ✅ ElastiCache | ✅ ElastiCache | ✅ Native | ❌ Self-host |
| **.NET Support** | ✅ StackExchange | ✅ EnyimMemcached | ✅ SDK | ✅ Client |
| **Overall Rating** | **9.5/10** | 7.0/10 | 7.5/10 | 8.0/10 |

### 3.4 Search Engines

| Feature | Elasticsearch | OpenSearch | Algolia | Solr |
|:--------|:--------------|:-----------|:--------|:-----|
| **Full-Text Search** | ✅ Excellent | ✅ Excellent | ✅ Excellent | ✅ Excellent |
| **Relevance Tuning** | ✅ BM25, Custom | ✅ BM25, Custom | ✅ AI-powered | ✅ BM25 |
| **Faceted Search** | ✅ Aggregations | ✅ Aggregations | ✅ Built-in | ✅ Built-in |
| **Autocomplete** | ✅ Suggest API | ✅ Suggest API | ✅ Built-in | ✅ Built-in |
| **Real-time** | ✅ Near real-time | ✅ Near real-time | ✅ Real-time | ⚠️ Commit delay |
| **AWS Managed** | ❌ Self-host | ✅ Native | ❌ SaaS | ❌ Self-host |
| **License** | SSPL | Apache 2.0 | Proprietary | Apache 2.0 |
| **.NET Support** | ✅ NEST/Elastic.Clients | ✅ Compatible | ✅ SDK | ✅ SolrNet |
| **Overall Rating** | 9.0/10 | **9.2/10** | 8.5/10 | 8.0/10 |

---

## 4. Selected Database: PostgreSQL (Amazon RDS)

### 4.1 Technology Overview

| Attribute | Details |
|:----------|:--------|
| **Name** | PostgreSQL |
| **Type** | Object-Relational Database |
| **Version** | 15.x |
| **AWS Service** | Amazon RDS for PostgreSQL |
| **License** | PostgreSQL License (MIT-like, Free) |
| **Official Website** | https://www.postgresql.org |

### 4.2 Why PostgreSQL for AmCart?

| Benefit | Description | Use Case |
|:--------|:------------|:---------|
| **ACID Compliance** | Full transaction support with isolation levels | Order and payment processing |
| **JSON/JSONB Support** | Native JSON storage with indexing | Product attributes, metadata |
| **Advanced Indexing** | B-tree, GiST, GIN, BRIN indexes | Fast queries on large tables |
| **Full-Text Search** | Built-in tsvector/tsquery | Backup search functionality |
| **Partitioning** | Native table partitioning | Order history by date |
| **Row-Level Security** | Fine-grained access control | Multi-tenant data isolation |
| **Foreign Keys** | Referential integrity | Data consistency |
| **CTEs & Window Functions** | Complex analytical queries | Reporting, analytics |

### 4.3 PostgreSQL Schema for AmCart

```sql
-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    gender VARCHAR(10) CHECK (gender IN ('male', 'female', 'other')),
    avatar_url TEXT,
    role VARCHAR(20) DEFAULT 'customer' CHECK (role IN ('customer', 'admin')),
    is_verified BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Products table with JSONB for flexible attributes
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sku VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    base_price DECIMAL(10, 2) NOT NULL,
    sale_price DECIMAL(10, 2),
    is_on_sale BOOLEAN DEFAULT FALSE,
    is_new BOOLEAN DEFAULT FALSE,
    is_featured BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    category_id UUID REFERENCES categories(id),
    brand_id UUID REFERENCES brands(id),
    attributes JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Orders table with partitioning by date
CREATE TABLE orders (
    id UUID NOT NULL,
    order_number VARCHAR(20) UNIQUE NOT NULL,
    user_id UUID REFERENCES users(id),
    shipping_address_id UUID REFERENCES addresses(id),
    billing_address_id UUID REFERENCES addresses(id),
    status VARCHAR(20) DEFAULT 'pending',
    subtotal DECIMAL(10, 2) NOT NULL,
    discount_amount DECIMAL(10, 2) DEFAULT 0,
    coupon_code VARCHAR(50),
    shipping_cost DECIMAL(10, 2) DEFAULT 0,
    tax_amount DECIMAL(10, 2) DEFAULT 0,
    total DECIMAL(10, 2) NOT NULL,
    notes TEXT,
    placed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    shipped_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    PRIMARY KEY (id, placed_at)
) PARTITION BY RANGE (placed_at);

-- Create partitions for orders
CREATE TABLE orders_2024_q1 PARTITION OF orders
    FOR VALUES FROM ('2024-01-01') TO ('2024-04-01');
CREATE TABLE orders_2024_q2 PARTITION OF orders
    FOR VALUES FROM ('2024-04-01') TO ('2024-07-01');
-- ... more partitions

-- Indexes for performance
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_brand ON products(brand_id);
CREATE INDEX idx_products_slug ON products(slug);
CREATE INDEX idx_products_is_active ON products(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_orders_user ON orders(user_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_placed_at ON orders(placed_at);

-- Full-text search index on products
CREATE INDEX idx_products_search ON products 
    USING GIN(to_tsvector('english', name || ' ' || COALESCE(description, '')));
```

### 4.4 AWS RDS Configuration

| Setting | Value | Rationale |
|:--------|:------|:----------|
| **Instance Class** | db.t3.medium (dev) / db.r6g.large (prod) | Cost-performance balance |
| **Storage** | 100GB gp3 SSD | Fast I/O, expandable |
| **Multi-AZ** | Yes (production) | High availability |
| **Read Replicas** | 1-2 (production) | Read scaling |
| **Backup Retention** | 7 days | Point-in-time recovery |
| **Encryption** | Yes (AES-256) | Data at rest encryption |
| **Parameter Group** | Custom | Tuned for workload |

### 4.5 Connection from .NET

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=amcart-db.xxx.rds.amazonaws.com;Database=amcart;Username=amcart_user;Password=xxx;SSL Mode=Require;"
  }
}

// Program.cs
builder.Services.AddDbContext<AmCartDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        }));

// Add health check
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);
```

---

## 5. Selected Database: MongoDB (Amazon DocumentDB)

### 5.1 Technology Overview

| Attribute | Details |
|:----------|:--------|
| **Name** | Amazon DocumentDB |
| **Type** | Document Database (MongoDB-compatible) |
| **Compatibility** | MongoDB 4.0, 5.0 |
| **AWS Service** | Amazon DocumentDB |
| **License** | AWS Proprietary (MongoDB API compatible) |

### 5.2 Why DocumentDB for AmCart?

| Benefit | Description | Use Case |
|:--------|:------------|:---------|
| **Flexible Schema** | No schema migrations required | Reviews with varying fields |
| **Nested Documents** | Store related data together | Review replies, user activity |
| **AWS Managed** | Automatic patching, backups | Reduced operations |
| **MongoDB Compatible** | Use existing MongoDB drivers | Easy development |
| **Auto-Scaling Storage** | Storage grows automatically | Handle growth |
| **High Availability** | 6 copies across 3 AZs | Durability |

### 5.3 DocumentDB Collections for AmCart

#### Reviews Collection

```javascript
// Collection: reviews
{
  _id: ObjectId("..."),
  productId: "uuid-string",
  userId: "uuid-string",
  userName: "John Doe",
  userAvatar: "https://s3.../avatar.jpg",
  rating: 4.5,
  title: "Excellent quality!",
  content: "The fabric is really soft and the fit is perfect...",
  images: [
    "https://s3.../review-1.jpg",
    "https://s3.../review-2.jpg"
  ],
  isVerifiedPurchase: true,
  helpful: {
    up: 15,
    down: 2
  },
  status: "approved", // pending, approved, rejected
  replies: [
    {
      userId: "uuid-seller",
      userName: "AmCart Support",
      content: "Thank you for your feedback!",
      createdAt: ISODate("2024-12-19T10:30:00Z")
    }
  ],
  createdAt: ISODate("2024-12-18T14:20:00Z"),
  updatedAt: ISODate("2024-12-19T10:30:00Z")
}

// Indexes
db.reviews.createIndex({ productId: 1, createdAt: -1 });
db.reviews.createIndex({ userId: 1 });
db.reviews.createIndex({ status: 1 });
db.reviews.createIndex({ rating: 1 });
```

#### Notifications Collection

```javascript
// Collection: notifications
{
  _id: ObjectId("..."),
  userId: "uuid-string",
  type: "order_shipped",
  channel: "email", // email, sms, push
  template: "order-shipped-template",
  subject: "Your order has been shipped!",
  data: {
    orderId: "ORD-2024-001234",
    orderNumber: "ORD-2024-001234",
    trackingNumber: "TRK123456789",
    carrier: "FedEx",
    estimatedDelivery: "2024-12-22",
    items: [
      { name: "Blue T-Shirt", quantity: 2 }
    ]
  },
  status: "sent", // pending, sent, failed, read
  attempts: 1,
  sentAt: ISODate("2024-12-19T08:00:00Z"),
  readAt: null,
  createdAt: ISODate("2024-12-19T07:55:00Z"),
  expiresAt: ISODate("2025-01-19T07:55:00Z") // TTL
}

// Indexes
db.notifications.createIndex({ userId: 1, createdAt: -1 });
db.notifications.createIndex({ status: 1 });
db.notifications.createIndex({ expiresAt: 1 }, { expireAfterSeconds: 0 }); // TTL index
```

#### Audit Logs Collection

```javascript
// Collection: audit_logs
{
  _id: ObjectId("..."),
  timestamp: ISODate("2024-12-19T10:15:30Z"),
  service: "order-service",
  action: "order.created",
  actor: {
    type: "user",
    id: "uuid-user",
    email: "customer@example.com",
    ip: "192.168.1.100"
  },
  resource: {
    type: "order",
    id: "uuid-order",
    orderId: "ORD-2024-001234"
  },
  changes: {
    status: { from: null, to: "pending" },
    total: { from: null, to: 2499.00 }
  },
  metadata: {
    userAgent: "Mozilla/5.0...",
    requestId: "req-uuid"
  }
}

// Indexes
db.audit_logs.createIndex({ timestamp: -1 });
db.audit_logs.createIndex({ "actor.id": 1 });
db.audit_logs.createIndex({ "resource.type": 1, "resource.id": 1 });
db.audit_logs.createIndex({ service: 1, action: 1 });
```

### 5.4 AWS DocumentDB Configuration

| Setting | Value | Rationale |
|:--------|:------|:----------|
| **Instance Class** | db.t3.medium (dev) / db.r6g.large (prod) | Cost-performance |
| **Instances** | 1 (dev) / 3 (prod) | High availability |
| **Storage Encryption** | Yes | Security compliance |
| **Backup Retention** | 7 days | Recovery |
| **TLS** | Required | Secure connections |

### 5.5 Connection from .NET

```csharp
// appsettings.json
{
  "MongoDB": {
    "ConnectionString": "mongodb://amcart:password@amcart-docdb.cluster-xxx.docdb.amazonaws.com:27017/?tls=true&tlsCAFile=global-bundle.pem&replicaSet=rs0&readPreference=secondaryPreferred",
    "DatabaseName": "amcart"
  }
}

// MongoDbSettings.cs
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}

// Program.cs
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});
```

---

## 6. Selected Database: Redis (Amazon ElastiCache)

### 6.1 Technology Overview

| Attribute | Details |
|:----------|:--------|
| **Name** | Redis |
| **Type** | In-Memory Data Store |
| **Version** | 7.x |
| **AWS Service** | Amazon ElastiCache for Redis |
| **License** | BSD (Open Source) |
| **Official Website** | https://redis.io |

### 6.2 Why Redis for AmCart?

| Benefit | Description | Use Case |
|:--------|:------------|:---------|
| **Sub-millisecond Latency** | In-memory operations | Session lookups, cart access |
| **Rich Data Structures** | Strings, Hashes, Lists, Sets, Sorted Sets | Complex caching patterns |
| **TTL Support** | Automatic expiration | Session timeout, cache invalidation |
| **Pub/Sub** | Real-time messaging | Live inventory updates |
| **Atomic Operations** | INCR, DECR, LPUSH | Rate limiting, counters |
| **Persistence** | RDB + AOF | Data durability |
| **Cluster Mode** | Horizontal scaling | Handle high throughput |

### 6.3 Redis Data Patterns for AmCart

#### Session Management

```
# Key pattern: session:{sessionId}
# Type: Hash
# TTL: 24 hours (86400 seconds)

HSET session:abc123 userId "uuid-123" email "user@example.com" role "customer" createdAt "2024-12-19T10:00:00Z"
EXPIRE session:abc123 86400

# Get session
HGETALL session:abc123
```

#### Shopping Cart

```
# Key pattern: cart:{userId}
# Type: Hash (product variants as fields)
# TTL: 7 days (604800 seconds)

# Add item to cart
HSET cart:user123 "variant:prod001:M:Blue" '{"productId":"prod001","variantId":"var001","name":"Blue T-Shirt","size":"M","color":"Blue","price":999,"quantity":2,"image":"https://..."}'

# Update quantity (using Lua script for atomicity)
EVAL "local item = redis.call('HGET', KEYS[1], ARGV[1]); if item then local data = cjson.decode(item); data.quantity = tonumber(ARGV[2]); return redis.call('HSET', KEYS[1], ARGV[1], cjson.encode(data)); end; return nil;" 1 cart:user123 "variant:prod001:M:Blue" 3

# Get entire cart
HGETALL cart:user123

# Remove item
HDEL cart:user123 "variant:prod001:M:Blue"

# Clear cart
DEL cart:user123

# Set TTL
EXPIRE cart:user123 604800
```

#### Product Cache

```
# Key pattern: product:{productId}
# Type: String (JSON)
# TTL: 1 hour (3600 seconds)

SET product:prod001 '{"id":"prod001","name":"Blue T-Shirt","slug":"blue-t-shirt","price":999,...}' EX 3600

# Get product
GET product:prod001

# Category cache
SET category:tree '{"categories":[...]}' EX 21600  # 6 hours
```

#### Real-time Inventory

```
# Key pattern: inventory:{variantId}
# Type: String (integer)
# No TTL (persistent)

# Set initial stock
SET inventory:var001 100

# Decrease stock (atomic)
DECR inventory:var001

# Decrease by quantity
DECRBY inventory:var001 2

# Check stock
GET inventory:var001

# Reserve stock (using Lua for atomicity)
EVAL "local stock = tonumber(redis.call('GET', KEYS[1])); if stock and stock >= tonumber(ARGV[1]) then return redis.call('DECRBY', KEYS[1], ARGV[1]); else return nil; end;" 1 inventory:var001 2
```

#### Rate Limiting

```
# Key pattern: rate:{ip}:{endpoint}
# Type: String (counter)
# TTL: 1 minute (60 seconds)

# Increment and check (sliding window)
EVAL "local current = redis.call('INCR', KEYS[1]); if current == 1 then redis.call('EXPIRE', KEYS[1], 60); end; return current;" 1 rate:192.168.1.1:/api/auth/login

# Check if rate limited (limit: 10 requests/minute)
# If returned value > 10, reject request
```

#### Pub/Sub for Real-time Updates

```
# Publish inventory change
PUBLISH inventory:updates '{"variantId":"var001","stock":98,"action":"decrement"}'

# Subscribe (in notification service)
SUBSCRIBE inventory:updates

# Publish order status
PUBLISH order:status '{"orderId":"ord001","status":"shipped","userId":"user123"}'
```

### 6.4 AWS ElastiCache Configuration

| Setting | Value | Rationale |
|:--------|:------|:----------|
| **Node Type** | cache.t3.micro (dev) / cache.r6g.large (prod) | Cost-performance |
| **Cluster Mode** | Disabled (dev) / Enabled (prod) | Scaling capability |
| **Replicas** | 0 (dev) / 2 (prod) | High availability |
| **Multi-AZ** | No (dev) / Yes (prod) | Failover |
| **Encryption at Rest** | Yes | Security |
| **Encryption in Transit** | Yes (TLS) | Security |
| **Backup** | Daily | Recovery |

### 6.5 Connection from .NET

```csharp
// appsettings.json
{
  "Redis": {
    "ConnectionString": "amcart-redis.xxx.cache.amazonaws.com:6379,ssl=true,abortConnect=false"
  }
}

// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "AmCart:";
});

// For direct Redis access
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(configuration!);
});

// Health check
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!);

// Usage in service
public class CartService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public CartService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task AddToCartAsync(string userId, CartItem item)
    {
        var key = $"cart:{userId}";
        var field = $"variant:{item.ProductId}:{item.Size}:{item.Color}";
        var value = JsonSerializer.Serialize(item);
        
        await _db.HashSetAsync(key, field, value);
        await _db.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task<List<CartItem>> GetCartAsync(string userId)
    {
        var key = $"cart:{userId}";
        var entries = await _db.HashGetAllAsync(key);
        
        return entries
            .Select(e => JsonSerializer.Deserialize<CartItem>(e.Value!))
            .Where(i => i != null)
            .ToList()!;
    }
}
```

---

## 7. Selected Database: OpenSearch (Amazon OpenSearch)

### 7.1 Technology Overview

| Attribute | Details |
|:----------|:--------|
| **Name** | Amazon OpenSearch Service |
| **Type** | Search & Analytics Engine |
| **Based On** | OpenSearch (Elasticsearch fork) |
| **Version** | OpenSearch 2.11 |
| **AWS Service** | Amazon OpenSearch Service |
| **License** | Apache 2.0 |

### 7.2 Why OpenSearch for AmCart?

| Benefit | Description | Use Case |
|:--------|:------------|:---------|
| **Full-Text Search** | BM25 relevance scoring | Product search |
| **Autocomplete** | Completion suggester | Search-as-you-type |
| **Faceted Search** | Aggregations | Filter by category, brand, price |
| **Fuzzy Matching** | Typo tolerance | "shirrt" → "shirt" |
| **Synonyms** | Query expansion | "tee" → "t-shirt" |
| **Real-time Indexing** | Near real-time updates | Product updates visible quickly |
| **AWS Managed** | Automatic operations | Reduced maintenance |

### 7.3 OpenSearch Index Mapping for Products

```json
PUT /products
{
  "settings": {
    "number_of_shards": 2,
    "number_of_replicas": 1,
    "analysis": {
      "analyzer": {
        "product_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "product_synonyms", "stemmer"]
        },
        "autocomplete_analyzer": {
          "type": "custom",
          "tokenizer": "autocomplete_tokenizer",
          "filter": ["lowercase"]
        }
      },
      "tokenizer": {
        "autocomplete_tokenizer": {
          "type": "edge_ngram",
          "min_gram": 2,
          "max_gram": 20,
          "token_chars": ["letter", "digit"]
        }
      },
      "filter": {
        "product_synonyms": {
          "type": "synonym",
          "synonyms": [
            "tee, t-shirt, tshirt",
            "jeans, denim, pants",
            "shirt, top, blouse"
          ]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "sku": { "type": "keyword" },
      "name": {
        "type": "text",
        "analyzer": "product_analyzer",
        "fields": {
          "autocomplete": {
            "type": "text",
            "analyzer": "autocomplete_analyzer",
            "search_analyzer": "standard"
          },
          "keyword": {
            "type": "keyword"
          }
        }
      },
      "description": {
        "type": "text",
        "analyzer": "product_analyzer"
      },
      "slug": { "type": "keyword" },
      "category": {
        "properties": {
          "id": { "type": "keyword" },
          "name": { "type": "keyword" },
          "path": { "type": "keyword" }
        }
      },
      "brand": {
        "properties": {
          "id": { "type": "keyword" },
          "name": { "type": "keyword" }
        }
      },
      "price": { "type": "float" },
      "salePrice": { "type": "float" },
      "isOnSale": { "type": "boolean" },
      "isNew": { "type": "boolean" },
      "isFeatured": { "type": "boolean" },
      "inStock": { "type": "boolean" },
      "colors": { "type": "keyword" },
      "sizes": { "type": "keyword" },
      "gender": { "type": "keyword" },
      "rating": { "type": "float" },
      "reviewCount": { "type": "integer" },
      "images": { "type": "keyword" },
      "createdAt": { "type": "date" },
      "updatedAt": { "type": "date" },
      "suggest": {
        "type": "completion",
        "analyzer": "simple",
        "preserve_separators": true,
        "preserve_position_increments": true,
        "max_input_length": 50
      }
    }
  }
}
```

### 7.4 Search Queries

#### Full-Text Search with Filters

```json
POST /products/_search
{
  "query": {
    "bool": {
      "must": [
        {
          "multi_match": {
            "query": "blue cotton shirt",
            "fields": ["name^3", "description", "brand.name"],
            "type": "best_fields",
            "fuzziness": "AUTO"
          }
        }
      ],
      "filter": [
        { "term": { "gender": "men" } },
        { "range": { "price": { "gte": 500, "lte": 2000 } } },
        { "term": { "inStock": true } },
        { "terms": { "sizes": ["M", "L"] } }
      ]
    }
  },
  "aggs": {
    "categories": {
      "terms": { "field": "category.name", "size": 20 }
    },
    "brands": {
      "terms": { "field": "brand.name", "size": 20 }
    },
    "colors": {
      "terms": { "field": "colors", "size": 20 }
    },
    "sizes": {
      "terms": { "field": "sizes", "size": 10 }
    },
    "price_ranges": {
      "range": {
        "field": "price",
        "ranges": [
          { "to": 500 },
          { "from": 500, "to": 1000 },
          { "from": 1000, "to": 2000 },
          { "from": 2000 }
        ]
      }
    }
  },
  "highlight": {
    "fields": {
      "name": {},
      "description": {}
    }
  },
  "from": 0,
  "size": 20,
  "sort": [
    { "_score": "desc" },
    { "rating": "desc" }
  ]
}
```

#### Autocomplete Query

```json
POST /products/_search
{
  "suggest": {
    "product-suggest": {
      "prefix": "blu",
      "completion": {
        "field": "suggest",
        "size": 10,
        "skip_duplicates": true,
        "fuzzy": {
          "fuzziness": 1
        }
      }
    }
  }
}
```

### 7.5 AWS OpenSearch Configuration

| Setting | Value | Rationale |
|:--------|:------|:----------|
| **Instance Type** | t3.small.search (dev) / r6g.large.search (prod) | Cost-performance |
| **Instance Count** | 1 (dev) / 2 (prod) | High availability |
| **Storage** | 50GB (dev) / 200GB (prod) | Data size |
| **Zone Awareness** | No (dev) / Yes (prod) | Multi-AZ |
| **Encryption** | Yes | Security |
| **Fine-Grained Access** | Yes | Role-based access |

### 7.6 Connection from .NET

```csharp
// appsettings.json
{
  "OpenSearch": {
    "Url": "https://amcart-search.us-east-1.es.amazonaws.com",
    "Username": "admin",
    "Password": "xxx"
  }
}

// Program.cs
builder.Services.AddSingleton<IElasticClient>(sp =>
{
    var settings = new ConnectionSettings(
        new Uri(builder.Configuration["OpenSearch:Url"]!))
        .BasicAuthentication(
            builder.Configuration["OpenSearch:Username"],
            builder.Configuration["OpenSearch:Password"])
        .DefaultIndex("products");
    
    return new ElasticClient(settings);
});

// Search service
public class ProductSearchService
{
    private readonly IElasticClient _client;

    public ProductSearchService(IElasticClient client)
    {
        _client = client;
    }

    public async Task<SearchResult<ProductDocument>> SearchAsync(
        string query, 
        ProductFilters filters, 
        int page = 1, 
        int pageSize = 20)
    {
        var response = await _client.SearchAsync<ProductDocument>(s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields(f => f
                                .Field(p => p.Name, boost: 3)
                                .Field(p => p.Description)
                                .Field(p => p.Brand.Name))
                            .Fuzziness(Fuzziness.Auto)))
                    .Filter(BuildFilters(filters))))
            .Aggregations(a => a
                .Terms("categories", t => t.Field(f => f.Category.Name))
                .Terms("brands", t => t.Field(f => f.Brand.Name))
                .Terms("colors", t => t.Field(f => f.Colors))
                .Terms("sizes", t => t.Field(f => f.Sizes)))
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Highlight(h => h
                .Fields(f => f
                    .Field(p => p.Name)
                    .Field(p => p.Description))));

        return new SearchResult<ProductDocument>
        {
            Items = response.Documents.ToList(),
            Total = response.Total,
            Aggregations = MapAggregations(response.Aggregations)
        };
    }
}
```

---

## 8. Service-to-Database Mapping

### 8.1 Complete Mapping

| Microservice | Primary DB | Secondary DB | Cache | Search |
|:-------------|:-----------|:-------------|:------|:-------|
| **UserService** | PostgreSQL | - | Redis (sessions) | - |
| **ProductService** | PostgreSQL | - | Redis (cache) | OpenSearch |
| **CartService** | Redis | PostgreSQL (backup) | - | - |
| **OrderService** | PostgreSQL | - | Redis (cache) | - |
| **PaymentService** | PostgreSQL | - | - | - |
| **InventoryService** | PostgreSQL | - | Redis (real-time) | - |
| **SearchService** | - | - | Redis (cache) | OpenSearch |
| **NotificationService** | DocumentDB | - | Redis (queue) | - |
| **ReviewService** | DocumentDB | - | Redis (cache) | - |

### 8.2 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Data Flow Architecture                             │
│                                                                              │
│   ┌─────────────┐         ┌─────────────┐         ┌─────────────┐          │
│   │   Product   │ ──1───▶ │ PostgreSQL  │ ──2───▶ │  OpenSearch │          │
│   │   Service   │         │   (RDS)     │         │  (Indexer)  │          │
│   └─────────────┘         └─────────────┘         └─────────────┘          │
│         │                                                │                  │
│         │ 3                                              │ 4                │
│         ▼                                                ▼                  │
│   ┌─────────────┐                                 ┌─────────────┐          │
│   │    Redis    │                                 │   Search    │          │
│   │   (Cache)   │                                 │   Service   │          │
│   └─────────────┘                                 └─────────────┘          │
│                                                                              │
│   Flow:                                                                      │
│   1. Product Service writes to PostgreSQL (source of truth)                 │
│   2. Change Data Capture syncs to OpenSearch                                │
│   3. Product Service caches hot data in Redis                               │
│   4. Search Service queries OpenSearch for search requests                  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 9. Cost Analysis (AWS - Monthly)

### 9.1 Development Environment

| Service | Configuration | Monthly Cost |
|:--------|:--------------|:-------------|
| **RDS PostgreSQL** | db.t3.micro, 20GB, Single-AZ | $15 |
| **DocumentDB** | db.t3.medium (single instance) | $60 |
| **ElastiCache Redis** | cache.t3.micro (single node) | $12 |
| **OpenSearch** | t3.small.search (single node), 20GB | $35 |
| **TOTAL (Dev)** | | **~$122/month** |

### 9.2 Production Environment

| Service | Configuration | Monthly Cost |
|:--------|:--------------|:-------------|
| **RDS PostgreSQL** | db.r6g.large, 100GB, Multi-AZ, 1 Read Replica | $350 |
| **DocumentDB** | db.r6g.large (3 instances) | $400 |
| **ElastiCache Redis** | cache.r6g.large, 2 replicas, Cluster Mode | $300 |
| **OpenSearch** | r6g.large.search (2 nodes), 100GB | $400 |
| **Backup Storage** | 500GB total | $25 |
| **Data Transfer** | Inter-AZ, VPC | $50 |
| **TOTAL (Prod)** | | **~$1,525/month** |

### 9.3 Cost Optimization Strategies

| Strategy | Potential Savings |
|:---------|:------------------|
| Reserved Instances (1-year) | 30-40% |
| Use smaller dev instances | 50% |
| Optimize storage (gp3 vs io1) | 20% |
| Right-size based on metrics | 20-30% |
| Use read replicas only if needed | Variable |

---

## 10. Data Backup & Recovery

### 10.1 Backup Strategy

| Database | Backup Type | Frequency | Retention |
|:---------|:------------|:----------|:----------|
| **PostgreSQL (RDS)** | Automated snapshots | Daily | 7 days |
| **PostgreSQL (RDS)** | Point-in-time recovery | Continuous | 7 days |
| **DocumentDB** | Automated snapshots | Daily | 7 days |
| **ElastiCache Redis** | RDB snapshots | Daily | 3 days |
| **OpenSearch** | Automated snapshots | Hourly | 14 days |

### 10.2 Recovery Time Objectives

| Database | RPO (Recovery Point Objective) | RTO (Recovery Time Objective) |
|:---------|:-------------------------------|:------------------------------|
| **PostgreSQL** | < 5 minutes (PITR) | < 30 minutes |
| **DocumentDB** | < 1 hour | < 1 hour |
| **Redis** | < 1 day (data is cacheable) | < 15 minutes |
| **OpenSearch** | < 1 hour | < 30 minutes |

---

## 11. Security Considerations

### 11.1 Security Measures by Database

| Database | Encryption at Rest | Encryption in Transit | Access Control |
|:---------|:-------------------|:----------------------|:---------------|
| **PostgreSQL** | AES-256 (RDS) | TLS 1.2+ | IAM + DB users |
| **DocumentDB** | AES-256 | TLS required | IAM + DB users |
| **Redis** | AES-256 (ElastiCache) | TLS 1.2+ | AUTH token |
| **OpenSearch** | AES-256 | TLS 1.2+ | Fine-grained IAM |

### 11.2 Network Security

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           VPC Security Architecture                          │
│                                                                              │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │                        Private Subnets                               │   │
│   │                                                                      │   │
│   │   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐            │   │
│   │   │ EKS Pods    │───▶│   RDS       │    │ DocumentDB  │            │   │
│   │   │             │    │ (SG: 5432)  │    │ (SG: 27017) │            │   │
│   │   └─────────────┘    └─────────────┘    └─────────────┘            │   │
│   │          │                                                          │   │
│   │          │           ┌─────────────┐    ┌─────────────┐            │   │
│   │          └──────────▶│ ElastiCache │    │ OpenSearch  │            │   │
│   │                      │ (SG: 6379)  │    │ (SG: 443)   │            │   │
│   │                      └─────────────┘    └─────────────┘            │   │
│   │                                                                      │   │
│   │   Security Groups:                                                   │   │
│   │   - Only EKS pods can access databases                              │   │
│   │   - No public access to any database                                │   │
│   │   - All traffic within VPC                                          │   │
│   │                                                                      │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 12. Approval Signatures

### 12.1 Document Approval

| Role | Name | Signature | Date |
|:-----|:-----|:----------|:-----|
| **Technical Lead** | | | |
| **Database Administrator** | | | |
| **Solution Architect** | | | |
| **Project Manager** | | | |
| **Client Representative** | | | |

### 12.2 Decision Approval

| Approver | Decision | Comments |
|:---------|:---------|:---------|
| Technical Lead | ☐ Approved ☐ Rejected | |
| DBA | ☐ Approved ☐ Rejected | |
| Solution Architect | ☐ Approved ☐ Rejected | |
| Client | ☐ Approved ☐ Rejected | |

---

## 13. Appendices

### Appendix A: Database Selection Decision Tree

```
                    ┌─────────────────────────────┐
                    │   What type of data?        │
                    └─────────────────────────────┘
                                  │
            ┌─────────────────────┼─────────────────────┐
            ▼                     ▼                     ▼
    ┌───────────────┐    ┌───────────────┐    ┌───────────────┐
    │ Transactional │    │   Flexible    │    │   Temporary   │
    │   (ACID)      │    │   Schema      │    │   (Cache)     │
    └───────────────┘    └───────────────┘    └───────────────┘
            │                     │                     │
            ▼                     ▼                     ▼
    ┌───────────────┐    ┌───────────────┐    ┌───────────────┐
    │  PostgreSQL   │    │   MongoDB     │    │    Redis      │
    │    (RDS)      │    │ (DocumentDB)  │    │(ElastiCache)  │
    └───────────────┘    └───────────────┘    └───────────────┘
    
    
                    ┌─────────────────────────────┐
                    │   Need full-text search?    │
                    └─────────────────────────────┘
                                  │
                                  ▼
                    ┌───────────────────────────┐
                    │        OpenSearch         │
                    └───────────────────────────┘
```

### Appendix B: Reference Links

| Resource | URL |
|:---------|:----|
| PostgreSQL Documentation | https://www.postgresql.org/docs |
| Amazon RDS for PostgreSQL | https://aws.amazon.com/rds/postgresql |
| Amazon DocumentDB | https://aws.amazon.com/documentdb |
| Amazon ElastiCache for Redis | https://aws.amazon.com/elasticache/redis |
| Amazon OpenSearch Service | https://aws.amazon.com/opensearch-service |
| Npgsql (.NET PostgreSQL) | https://www.npgsql.org |
| MongoDB C# Driver | https://mongodb.github.io/mongo-csharp-driver |
| StackExchange.Redis | https://stackexchange.github.io/StackExchange.Redis |
| NEST (Elasticsearch .NET) | https://www.elastic.co/guide/en/elasticsearch/client/net-api |

### Appendix C: Revision History

| Version | Date | Author | Changes |
|:--------|:-----|:-------|:--------|
| 1.0 | Dec 19, 2024 | Technical Team | Initial document creation |

---

**END OF DOCUMENT**

