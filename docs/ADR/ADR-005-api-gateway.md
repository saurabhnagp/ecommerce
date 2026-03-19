# ADR-005: API Gateway Selection

## Status
**Accepted**

## Date
2024-12-19

## Context

With a microservices architecture, we need an API Gateway to:
- Route requests to appropriate services
- Handle cross-cutting concerns (auth, rate limiting, CORS)
- Provide SSL termination
- Load balance across service instances
- Aggregate/transform responses (if needed)

Requirements:
- High performance (low latency overhead)
- Easy configuration
- Rate limiting capabilities
- Health checking
- Docker/Kubernetes compatible
- Cost-effective

## Decision

We will use **Nginx** as the API Gateway / Reverse Proxy.

### Configuration

Nginx will handle:
1. **Routing**: Path-based routing to microservices
2. **Rate Limiting**: Using `limit_req_zone`
3. **SSL Termination**: With Let's Encrypt certificates
4. **Load Balancing**: Round-robin/least-connections
5. **Health Checks**: Upstream health monitoring
6. **Caching**: Response caching for static data
7. **Compression**: Gzip/Brotli compression
8. **Security Headers**: X-Frame-Options, CSP, etc.

### Routing Configuration

```nginx
location /api/v1/auth/     → user-service:5001
location /api/v1/users/    → user-service:5001
location /api/v1/products/ → product-service:5002
location /api/v1/cart/     → cart-service:5003
location /api/v1/orders/   → order-service:5004
location /api/v1/payments/ → payment-service:5005
location /api/v1/search/   → search-service:5007
location /api/v1/reviews/  → review-service:5009
```

## Consequences

### Positive

- **Proven Technology**: Powers 30%+ of websites globally
- **High Performance**: Can handle 100k+ concurrent connections
- **Simple Configuration**: Easy to understand and maintain
- **Low Resource Usage**: Minimal CPU/memory footprint
- **Free & Open Source**: No licensing costs
- **Excellent Documentation**: Well-documented
- **Kubernetes Native**: Works well with Ingress Controller

### Negative

- **Limited Plugin Ecosystem**: Fewer out-of-box plugins than Kong
- **No GUI**: Configuration is file-based
- **Manual JWT Validation**: Need to implement or use modules
- **No Built-in Analytics**: Need external tools

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Limited Plugins | Use Lua modules or handle in services |
| No GUI | Use Infrastructure as Code (Terraform, Helm) |
| JWT Validation | Validate JWT in services, not gateway |
| Analytics | Use CloudWatch, Prometheus for metrics |

## Alternatives Considered

### 1. Kong

**Pros:**
- Rich plugin ecosystem
- Built-in JWT validation
- Admin API and GUI
- Rate limiting built-in

**Cons:**
- More complex setup
- Higher resource usage
- Overkill for our needs

**Why Rejected:** Complexity not justified for current requirements.

### 2. AWS API Gateway

**Pros:**
- Fully managed
- Native AWS integration
- Built-in throttling

**Cons:**
- Pay per request (can be expensive)
- Vendor lock-in
- Higher latency

**Why Rejected:** Cost concerns and vendor lock-in.

### 3. Ocelot (.NET)

**Pros:**
- .NET native
- Easy integration with services
- Configuration in C#

**Cons:**
- Less performant than Nginx
- Limited feature set
- Single-threaded concerns

**Why Rejected:** Performance concerns for high-traffic ecommerce.

### 4. Traefik

**Pros:**
- Cloud-native
- Auto-discovery in Kubernetes
- Modern architecture

**Cons:**
- More complex configuration
- Smaller community than Nginx
- Learning curve

**Why Rejected:** Team more familiar with Nginx.

## Implementation Notes

### Sample Nginx Configuration

```nginx
upstream user_service {
    least_conn;
    server user-service.amcart.svc.cluster.local:5001;
    keepalive 32;
}

limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/s;

server {
    listen 80;
    server_name api.amcart.com;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;

    # Health check
    location /health {
        return 200 'OK';
    }

    # API routes
    location /api/v1/users/ {
        limit_req zone=api_limit burst=50 nodelay;
        proxy_pass http://user_service;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Request-ID $request_id;
    }
}
```

### Kubernetes Deployment

Nginx will be deployed as:
- Kubernetes Ingress Controller (nginx-ingress)
- Or as a separate deployment for more control

## References

- [Nginx Documentation](https://nginx.org/en/docs/)
- [Nginx Ingress Controller](https://kubernetes.github.io/ingress-nginx/)
- [Nginx Rate Limiting](https://www.nginx.com/blog/rate-limiting-nginx/)

