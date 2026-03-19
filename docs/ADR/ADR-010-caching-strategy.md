# ADR-010: Caching Strategy

## Status
**Accepted**

## Date
2024-12-19

## Context

AmCart needs caching for:
- Reducing database load
- Improving response times
- Session management
- Shopping cart (temporary data)
- Rate limiting
- Product catalog (frequently accessed)

Without caching:
- Database becomes bottleneck
- Higher latency for users
- Higher infrastructure costs

## Decision

We will implement a **multi-layer caching strategy** using Redis (Amazon ElastiCache).

### Caching Layers

```
┌─────────────────────────────────────────────────────────────┐
│                      Client (Browser)                        │
│           HTTP Cache, Service Worker Cache                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      CDN (CloudFront)                        │
│        Static assets, product images, API responses          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Nginx (Reverse Proxy)                     │
│           Micro-caching for GET requests                     │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Redis (ElastiCache)                       │
│     Sessions, Cart, API Response Cache, Rate Limits          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Memory                        │
│          In-process cache (IMemoryCache)                     │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Database                              │
│           PostgreSQL, MongoDB, OpenSearch                    │
└─────────────────────────────────────────────────────────────┘
```

### Cache Categories

| Category | Storage | TTL | Example |
|----------|---------|-----|---------|
| Session | Redis | 30 min | User sessions |
| Cart | Redis | 7 days | Shopping cart |
| API Response | Redis | 5-60 min | Product listings |
| Static | CDN | 1 year | Images, CSS, JS |
| Rate Limit | Redis | 1 min | API rate limits |
| Application | Memory | 1-5 min | Config, lookups |

### Cache Key Design

```
Pattern: {service}:{type}:{identifier}:{version}

Examples:
- product:detail:uuid:v1
- product:list:category:electronics:page:1:v1
- cart:user:uuid
- session:token:abc123
- ratelimit:ip:1.2.3.4
- user:profile:uuid:v1
```

## Consequences

### Positive

- **Performance**: Sub-millisecond reads from Redis
- **Scalability**: Reduces database load significantly
- **Cost Efficiency**: Fewer database queries
- **User Experience**: Faster page loads
- **Resilience**: Can serve cached data if DB is slow

### Negative

- **Cache Invalidation**: "One of the hardest problems in CS"
- **Memory Costs**: Redis memory is expensive
- **Stale Data**: Risk of showing outdated information
- **Complexity**: Additional infrastructure layer

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Invalidation | Event-driven invalidation, short TTLs |
| Memory Cost | Cache only hot data, eviction policies |
| Stale Data | Short TTLs, background refresh |
| Complexity | Use caching abstraction libraries |

## Implementation

### Redis Configuration

```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "AmCart:";
});

// For distributed locking and advanced features
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
```

### Caching Service

```csharp
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached is null) return default;
        
        return JsonSerializer.Deserialize<T>(cached, JsonOptions);
    }

    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null, 
        CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };
        
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await _cache.SetStringAsync(key, json, options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null) return cached;

        var value = await factory();
        await SetAsync(key, value, expiration, ct);
        return value;
    }
}
```

### Product Service Example

```csharp
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly ICacheService _cache;
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public async Task<ProductDto?> Handle(GetProductQuery request, CancellationToken ct)
    {
        var cacheKey = $"product:detail:{request.ProductId}:v1";
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var product = await _repository.GetByIdAsync(request.ProductId, ct);
                return product is null ? null : _mapper.Map<ProductDto>(product);
            },
            TimeSpan.FromMinutes(30),
            ct);
    }
}
```

### Cache Invalidation on Update

```csharp
public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cache;
    private readonly IPublishEndpoint _publisher;

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        // Update database
        var product = await _repository.GetByIdAsync(request.ProductId, ct);
        product.Update(request.Name, request.Price, request.Description);
        await _repository.UpdateAsync(product, ct);

        // Invalidate caches
        await _cache.RemoveAsync($"product:detail:{request.ProductId}:v1", ct);
        await _cache.RemoveAsync($"product:list:category:{product.CategoryId}:*", ct);

        // Publish event for other services
        await _publisher.Publish(new ProductUpdatedEvent(product.Id), ct);

        return _mapper.Map<ProductDto>(product);
    }
}
```

### Shopping Cart in Redis

```csharp
public class CartService : ICartService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public CartService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<Cart> GetCartAsync(Guid userId)
    {
        var key = $"cart:user:{userId}";
        var data = await _db.StringGetAsync(key);
        
        if (data.IsNullOrEmpty)
            return new Cart(userId);

        return JsonSerializer.Deserialize<Cart>(data!)!;
    }

    public async Task SaveCartAsync(Cart cart)
    {
        var key = $"cart:user:{cart.UserId}";
        var json = JsonSerializer.Serialize(cart);
        
        await _db.StringSetAsync(key, json, TimeSpan.FromDays(7));
    }

    public async Task AddItemAsync(Guid userId, CartItem item)
    {
        var cart = await GetCartAsync(userId);
        cart.AddItem(item);
        await SaveCartAsync(cart);
    }
}
```

## Cache Invalidation Strategies

| Strategy | When to Use | Example |
|----------|-------------|---------|
| Time-based (TTL) | Data with known staleness tolerance | Product listings (5 min) |
| Event-driven | Critical data that must be fresh | Price updates |
| Write-through | Important data | User profile |
| Cache-aside | General use | Product details |
| Background refresh | High-traffic data | Homepage products |

## Alternatives Considered

### 1. Memcached

**Pros:**
- Simpler than Redis
- Multi-threaded

**Cons:**
- No persistence
- No data structures
- Limited features

**Why Rejected:** Redis provides more features needed for sessions and carts.

### 2. Application-only Caching

**Pros:**
- No external dependency
- Simpler architecture

**Cons:**
- Cache not shared across instances
- Memory pressure on app servers
- No persistence

**Why Rejected:** Multiple service instances need shared cache.

### 3. Database Query Caching

**Pros:**
- Transparent
- Database-managed

**Cons:**
- Limited control
- Still database connection overhead

**Why Rejected:** Not sufficient for our performance needs.

## References

- [Redis Documentation](https://redis.io/documentation)
- [Amazon ElastiCache](https://docs.aws.amazon.com/elasticache/)
- [Caching Patterns](https://docs.microsoft.com/azure/architecture/patterns/cache-aside)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)

