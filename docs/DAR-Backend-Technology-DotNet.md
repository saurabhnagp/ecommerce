# DECISION ANALYSIS AND RESOLUTION (DAR) DOCUMENT

## Backend Technology Selection - .NET 8 Microservices with Nginx Reverse Proxy

---

## Document Control Information

| Field | Value |
|:------|:------|
| **Document Title** | Backend Technology Selection - .NET 8 Microservices |
| **Document ID** | DAR-AMCART-BE-001 |
| **Project Name** | AmCart Ecommerce Platform |
| **Version** | 1.1 |
| **Status** | Approved |
| **Created Date** | December 19, 2024 |
| **Last Updated** | December 19, 2024 |
| **Prepared By** | Technical Architecture Team |
| **Reviewed By** | [Reviewer Name] |
| **Approved By** | [Approver Name] |

---

## 1. Executive Summary

### 1.1 Purpose

This Decision Analysis and Resolution (DAR) document records the evaluation and selection of **.NET 8** as the primary backend technology and **Nginx** as the reverse proxy/API gateway for the AmCart Ecommerce Platform microservices architecture, deployed on **AWS** using Docker and Kubernetes.

### 1.2 Decision Statement

After comprehensive evaluation of available backend technologies against project requirements and team expertise, the following technologies have been selected:

- **Reverse Proxy/API Gateway**: Nginx
- **Backend Framework**: .NET 8 (ASP.NET Core)
- **Cloud Provider**: AWS (Amazon Web Services)
- **Container Orchestration**: Docker + Kubernetes (EKS)
- **Relational Database**: PostgreSQL (Amazon RDS)
- **Document Database**: MongoDB (Amazon DocumentDB / Self-hosted)
- **Cache/Session Store**: Redis (Amazon ElastiCache)
- **Search Engine**: Elasticsearch (Amazon OpenSearch)

### 1.3 Decision Summary

| Component | Selected Technology | Version |
|:----------|:--------------------|:--------|
| **Reverse Proxy** | Nginx | 1.25.x |
| **Backend Framework** | .NET | 8.0 LTS |
| **Language** | C# | 12.0 |
| **IDE** | Visual Studio / Rider | Latest |
| **ORM** | Entity Framework Core | 8.0 |
| **API Documentation** | Swagger/OpenAPI | Latest |
| **Service Discovery** | Consul / AWS Cloud Map | Latest |
| **Message Broker** | RabbitMQ / Amazon SQS | Latest |
| **Cloud Provider** | AWS | - |

---

## 2. Business Context

### 2.1 Project Background

AmCart is a new ecommerce platform requiring a scalable, maintainable, and high-performance backend architecture. The development team has strong expertise in .NET ecosystem, making it the optimal choice for rapid and efficient development.

### 2.2 Business Objectives

| ID | Objective | Priority |
|:---|:----------|:---------|
| BO-01 | Leverage existing team .NET expertise | High |
| BO-02 | Support high transaction volumes during sales/promotions | High |
| BO-03 | Ensure 99.9% uptime for critical services | High |
| BO-04 | Enable independent deployment of services | High |
| BO-05 | Utilize AWS managed services for reduced operational overhead | High |
| BO-06 | Support future scaling as user base grows | Medium |

### 2.3 Technical Requirements

| ID | Requirement | Category |
|:---|:------------|:---------|
| TR-01 | Microservices architecture with independent deployability | Mandatory |
| TR-02 | Support for PostgreSQL, MongoDB, Redis, Elasticsearch | Mandatory |
| TR-03 | RESTful API design with OpenAPI documentation | Mandatory |
| TR-04 | Razorpay payment gateway integration | Mandatory |
| TR-05 | OAuth 2.0 / JWT authentication | Mandatory |
| TR-06 | Docker containerization with Kubernetes orchestration | Mandatory |
| TR-07 | AWS cloud deployment | Mandatory |
| TR-08 | Horizontal scalability | Mandatory |
| TR-09 | Circuit breaker pattern for fault tolerance | Desirable |
| TR-10 | Event-driven communication between services | Desirable |

---

## 3. Alternatives Considered

### 3.1 Backend Framework Alternatives

| Alternative | Description | Consideration Status |
|:------------|:------------|:---------------------|
| **.NET 8** | Microsoft's cross-platform framework | Selected |
| **Spring Boot** | Java-based enterprise framework | Evaluated - Team lacks Java expertise |
| **NestJS** | Node.js TypeScript framework | Evaluated |
| **Go (Gin/Fiber)** | High-performance compiled language | Evaluated |
| **Python FastAPI** | Modern async Python framework | Evaluated |

### 3.2 Reverse Proxy/API Gateway Alternatives

| Alternative | Description | Consideration Status |
|:------------|:------------|:---------------------|
| **Nginx** | High-performance reverse proxy | Selected |
| **Kong** | Enterprise API Gateway | Evaluated |
| **AWS API Gateway** | Managed cloud gateway | Evaluated |
| **Ocelot** | .NET API Gateway | Evaluated |
| **YARP** | .NET Reverse Proxy | Evaluated |

### 3.3 Why .NET 8 Was Selected

| Factor | .NET 8 | Spring Boot | NestJS | Go |
|:-------|:-------|:------------|:-------|:---|
| **Team Expertise** | Excellent | None | Basic | None |
| **Performance** | Excellent | Very Good | Good | Excellent |
| **Microservices Support** | Excellent | Excellent | Good | Good |
| **Cloud Integration** | Excellent (AWS SDK) | Good | Good | Good |
| **Development Speed** | Fast (familiar) | Slow (learning) | Medium | Slow |
| **Tooling** | Excellent | Excellent | Good | Good |
| **Long-term Support** | LTS (3 years) | LTS | Good | Excellent |

### 3.4 Why Nginx Was Selected Over Alternatives

| Factor | Nginx | Kong | AWS API Gateway | Ocelot |
|:-------|:------|:-----|:----------------|:-------|
| **Performance** | Excellent | Excellent | Good | Good |
| **Simplicity** | High | Medium | High | High |
| **Cost** | Free | Free/Paid | Pay per request | Free |
| **AWS Integration** | Good | Good | Excellent | Good |
| **Load Balancing** | Excellent | Excellent | Built-in | Basic |
| **SSL Termination** | Yes | Yes | Yes | Yes |
| **Configuration** | Simple | Complex | GUI/IaC | Code |
| **Vendor Lock-in** | None | None | AWS | None |

---

## 4. Selected Technology: .NET 8

### 4.1 Technology Overview

| Attribute | Details |
|:----------|:--------|
| **Name** | .NET 8 |
| **Type** | Cross-Platform Application Framework |
| **Language** | C# 12 |
| **Vendor** | Microsoft |
| **License** | MIT (Open Source) |
| **Release Date** | November 2023 |
| **Support** | LTS (Long Term Support) until November 2026 |
| **Official Website** | https://dotnet.microsoft.com |

### 4.2 Core Features

| Feature | Description | Benefit for AmCart |
|:--------|:------------|:-------------------|
| **Cross-Platform** | Runs on Windows, Linux, macOS | Deploy on Linux containers in AWS |
| **High Performance** | One of the fastest frameworks | Handle high traffic during sales |
| **Minimal APIs** | Lightweight API development | Quick microservice development |
| **Native AOT** | Ahead-of-time compilation | Faster startup, lower memory |
| **Entity Framework Core** | Powerful ORM | Easy PostgreSQL/MongoDB integration |
| **ASP.NET Core Identity** | Built-in auth system | User authentication, JWT support |
| **Built-in DI** | Dependency injection | Clean, testable architecture |
| **gRPC Support** | High-performance RPC | Inter-service communication |
| **Health Checks** | Built-in health endpoints | Kubernetes readiness/liveness |
| **OpenTelemetry** | Observability support | Distributed tracing, metrics |

### 4.3 .NET Microservices Libraries

| Library | Purpose | NuGet Package |
|:--------|:--------|:--------------|
| **MediatR** | CQRS & Mediator pattern | MediatR |
| **FluentValidation** | Request validation | FluentValidation.AspNetCore |
| **AutoMapper** | Object mapping | AutoMapper |
| **Polly** | Resilience & fault handling | Polly |
| **Serilog** | Structured logging | Serilog.AspNetCore |
| **MassTransit** | Message broker abstraction | MassTransit.RabbitMQ |
| **HealthChecks** | Health monitoring | AspNetCore.HealthChecks.* |
| **Swagger** | API documentation | Swashbuckle.AspNetCore |

---

## 5. Selected Technology: Nginx

### 5.1 Technology Overview

| Attribute | Details |
|:----------|:--------|
| **Name** | Nginx |
| **Type** | Web Server / Reverse Proxy |
| **Vendor** | F5 Networks |
| **License** | BSD (Open Source) |
| **Current Version** | 1.25.x |
| **Official Website** | https://nginx.org |

### 5.2 Nginx Features for AmCart

| Feature | Description | Use Case |
|:--------|:------------|:---------|
| **Reverse Proxy** | Route requests to microservices | API routing |
| **Load Balancing** | Distribute traffic across instances | High availability |
| **SSL Termination** | Handle HTTPS at edge | Secure connections |
| **Rate Limiting** | Throttle requests | API protection |
| **Caching** | Response caching | Reduce backend load |
| **Compression** | Gzip/Brotli compression | Faster responses |
| **Health Checks** | Backend health monitoring | Failover |
| **Access Logging** | Request logging | Debugging, audit |

### 5.3 Nginx Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Nginx Reverse Proxy                     │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                    SSL Termination                      │ │
│  │              (Let's Encrypt / ACM Certificates)         │ │
│  └────────────────────────────────────────────────────────┘ │
│                              │                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                   Rate Limiting                         │ │
│  │              (limit_req_zone, limit_conn)               │ │
│  └────────────────────────────────────────────────────────┘ │
│                              │                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                  Location Routing                       │ │
│  │                                                         │ │
│  │  /api/users/*      → upstream user_service              │ │
│  │  /api/products/*   → upstream product_service           │ │
│  │  /api/orders/*     → upstream order_service             │ │
│  │  /api/cart/*       → upstream cart_service              │ │
│  │  /api/payments/*   → upstream payment_service           │ │
│  │  /api/search/*     → upstream search_service            │ │
│  └────────────────────────────────────────────────────────┘ │
│                              │                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Upstream Load Balancing                    │ │
│  │         (Round Robin / Least Connections)               │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         ▼                     ▼                     ▼
   ┌──────────┐          ┌──────────┐          ┌──────────┐
   │  User    │          │ Product  │          │  Order   │
   │ Service  │          │ Service  │          │ Service  │
   │ (.NET)   │          │ (.NET)   │          │ (.NET)   │
   └──────────┘          └──────────┘          └──────────┘
```

### 5.4 Sample Nginx Configuration

```nginx
upstream user_service {
    least_conn;
    server user-service:5001;
    server user-service:5002;
    keepalive 32;
}

upstream product_service {
    least_conn;
    server product-service:5001;
    server product-service:5002;
    keepalive 32;
}

upstream order_service {
    least_conn;
    server order-service:5001;
    keepalive 32;
}

# Rate limiting zone
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
limit_req_zone $binary_remote_addr zone=auth_limit:10m rate=5r/s;

server {
    listen 80;
    listen 443 ssl http2;
    server_name api.amcart.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # Gzip compression
    gzip on;
    gzip_types application/json text/plain application/javascript;

    # Health check endpoint
    location /health {
        return 200 'OK';
        add_header Content-Type text/plain;
    }

    # Auth endpoints (stricter rate limit)
    location /api/v1/auth/ {
        limit_req zone=auth_limit burst=10 nodelay;
        proxy_pass http://user_service;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # User service
    location /api/v1/users/ {
        limit_req zone=api_limit burst=20 nodelay;
        proxy_pass http://user_service;
        include proxy_params;
    }

    # Product service
    location /api/v1/products/ {
        limit_req zone=api_limit burst=50 nodelay;
        proxy_pass http://product_service;
        include proxy_params;
        
        # Cache GET requests
        proxy_cache_valid 200 5m;
    }

    # Order service
    location /api/v1/orders/ {
        limit_req zone=api_limit burst=20 nodelay;
        proxy_pass http://order_service;
        include proxy_params;
    }

    # ... other locations
}
```

---

## 6. AWS Cloud Architecture

### 6.1 AWS Services Used

| Service | Purpose | Configuration |
|:--------|:--------|:--------------|
| **EKS** | Kubernetes orchestration | Managed control plane |
| **ECR** | Container registry | Store Docker images |
| **RDS (PostgreSQL)** | Relational database | Multi-AZ, Read replicas |
| **DocumentDB** | MongoDB-compatible | Or self-hosted MongoDB |
| **ElastiCache (Redis)** | Caching, sessions | Cluster mode |
| **OpenSearch** | Search engine | 3-node cluster |
| **SQS/SNS** | Message queuing | Or RabbitMQ on EC2 |
| **S3** | Object storage | Product images |
| **CloudFront** | CDN | Static assets, images |
| **Route 53** | DNS | Domain management |
| **ACM** | SSL certificates | HTTPS |
| **CloudWatch** | Monitoring & logging | Metrics, logs, alarms |
| **Secrets Manager** | Secret storage | API keys, DB passwords |
| **ALB** | Load balancer | In front of Nginx/EKS |

### 6.2 AWS Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              AWS Cloud                                       │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │                           Route 53 (DNS)                                 ││
│  │                     api.amcart.com → ALB                                 ││
│  └───────────────────────────────┬─────────────────────────────────────────┘│
│                                  │                                           │
│  ┌───────────────────────────────┼─────────────────────────────────────────┐│
│  │                    CloudFront (CDN)                                      ││
│  │              Static assets, product images                               ││
│  └───────────────────────────────┬─────────────────────────────────────────┘│
│                                  │                                           │
│  ┌───────────────────────────────┼─────────────────────────────────────────┐│
│  │              Application Load Balancer (ALB)                             ││
│  │                    SSL Termination                                       ││
│  └───────────────────────────────┬─────────────────────────────────────────┘│
│                                  │                                           │
│  ┌───────────────────────────────┼─────────────────────────────────────────┐│
│  │                        EKS Cluster                                       ││
│  │                                                                          ││
│  │  ┌─────────────────────────────────────────────────────────────────┐    ││
│  │  │                    Nginx Ingress Controller                      │    ││
│  │  └─────────────────────────────┬───────────────────────────────────┘    ││
│  │                                │                                         ││
│  │  ┌─────────────────────────────┼───────────────────────────────────┐    ││
│  │  │                   Microservices (.NET 8)                         │    ││
│  │  │                                                                  │    ││
│  │  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │    ││
│  │  │  │  User    │ │ Product  │ │   Cart   │ │  Order   │           │    ││
│  │  │  │ Service  │ │ Service  │ │ Service  │ │ Service  │           │    ││
│  │  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘           │    ││
│  │  │                                                                  │    ││
│  │  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │    ││
│  │  │  │ Payment  │ │Inventory │ │  Search  │ │  Notif   │           │    ││
│  │  │  │ Service  │ │ Service  │ │ Service  │ │ Service  │           │    ││
│  │  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘           │    ││
│  │  └──────────────────────────────────────────────────────────────────┘    ││
│  └──────────────────────────────────────────────────────────────────────────┘│
│                                  │                                           │
│  ┌───────────────────────────────┼─────────────────────────────────────────┐│
│  │                         Data Layer                                       ││
│  │                                                                          ││
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    ││
│  │  │ RDS         │  │ DocumentDB  │  │ ElastiCache │  │ OpenSearch  │    ││
│  │  │ PostgreSQL  │  │ MongoDB     │  │ Redis       │  │ Elastic     │    ││
│  │  │ Multi-AZ    │  │ Cluster     │  │ Cluster     │  │ 3-node      │    ││
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    ││
│  │                                                                          ││
│  │  ┌─────────────┐  ┌─────────────┐                                       ││
│  │  │    SQS      │  │     S3      │                                       ││
│  │  │  Queues     │  │   Storage   │                                       ││
│  │  └─────────────┘  └─────────────┘                                       ││
│  └──────────────────────────────────────────────────────────────────────────┘│
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Microservices Implementation (.NET 8)

### 7.1 Service Definitions

| Service | Port | Database | Responsibilities |
|:--------|:-----|:---------|:-----------------|
| **UserService** | 5001 | PostgreSQL, Redis | User management, authentication, profiles |
| **ProductService** | 5002 | PostgreSQL, OpenSearch, Redis | Product catalog, categories, brands |
| **CartService** | 5003 | Redis, PostgreSQL | Shopping cart CRUD |
| **OrderService** | 5004 | PostgreSQL | Order creation, status tracking |
| **PaymentService** | 5005 | PostgreSQL | Razorpay integration |
| **InventoryService** | 5006 | PostgreSQL, Redis | Stock management |
| **SearchService** | 5007 | OpenSearch | Full-text search |
| **NotificationService** | 5008 | MongoDB, Redis | Email, SMS notifications |
| **ReviewService** | 5009 | MongoDB | Product reviews, ratings |

### 7.2 Solution Structure

```
AmCart/
├── src/
│   ├── Services/
│   │   ├── UserService/
│   │   │   ├── UserService.Api/
│   │   │   │   ├── Controllers/
│   │   │   │   │   ├── AuthController.cs
│   │   │   │   │   ├── UsersController.cs
│   │   │   │   │   └── AddressesController.cs
│   │   │   │   ├── Program.cs
│   │   │   │   ├── appsettings.json
│   │   │   │   ├── Dockerfile
│   │   │   │   └── UserService.Api.csproj
│   │   │   ├── UserService.Application/
│   │   │   │   ├── Commands/
│   │   │   │   ├── Queries/
│   │   │   │   ├── Handlers/
│   │   │   │   ├── DTOs/
│   │   │   │   ├── Validators/
│   │   │   │   └── Mappings/
│   │   │   ├── UserService.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── User.cs
│   │   │   │   │   └── Address.cs
│   │   │   │   ├── Interfaces/
│   │   │   │   └── Events/
│   │   │   └── UserService.Infrastructure/
│   │   │       ├── Data/
│   │   │       │   ├── UserDbContext.cs
│   │   │       │   └── Migrations/
│   │   │       ├── Repositories/
│   │   │       └── Services/
│   │   │
│   │   ├── ProductService/
│   │   │   └── ... (same structure)
│   │   ├── CartService/
│   │   ├── OrderService/
│   │   ├── PaymentService/
│   │   ├── InventoryService/
│   │   ├── SearchService/
│   │   ├── NotificationService/
│   │   └── ReviewService/
│   │
│   ├── BuildingBlocks/
│   │   ├── Common/
│   │   │   ├── Common.Logging/
│   │   │   ├── Common.Messaging/
│   │   │   └── Common.Security/
│   │   └── EventBus/
│   │       ├── EventBus.Messages/
│   │       └── EventBus.RabbitMQ/
│   │
│   └── ApiGateway/
│       └── nginx/
│           ├── nginx.conf
│           └── Dockerfile
│
├── tests/
│   ├── UserService.UnitTests/
│   ├── UserService.IntegrationTests/
│   └── ... (per service)
│
├── deploy/
│   ├── docker/
│   │   └── docker-compose.yml
│   ├── k8s/
│   │   ├── base/
│   │   ├── overlays/
│   │   │   ├── dev/
│   │   │   └── prod/
│   │   └── kustomization.yaml
│   └── terraform/
│       ├── main.tf
│       ├── eks.tf
│       ├── rds.tf
│       └── variables.tf
│
├── AmCart.sln
└── README.md
```

### 7.3 Sample .NET 8 Minimal API (UserService)

```csharp
// Program.cs
using UserService.Api;
using UserService.Application;
using UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
```

### 7.4 Sample Controller

```csharp
// Controllers/AuthController.cs
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<ActionResult>(
            success => Ok(success),
            error => BadRequest(error));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<ActionResult>(
            success => Ok(success),
            error => Unauthorized(error));
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _mediator.Send(new RefreshTokenCommand(userId!));
        return Ok(result);
    }
}
```

### 7.5 Sample Dockerfile (.NET 8)

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Services/UserService/UserService.Api/UserService.Api.csproj", "Services/UserService/UserService.Api/"]
COPY ["Services/UserService/UserService.Application/UserService.Application.csproj", "Services/UserService/UserService.Application/"]
COPY ["Services/UserService/UserService.Domain/UserService.Domain.csproj", "Services/UserService/UserService.Domain/"]
COPY ["Services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "Services/UserService/UserService.Infrastructure/"]
COPY ["BuildingBlocks/Common/Common.Logging/Common.Logging.csproj", "BuildingBlocks/Common/Common.Logging/"]

RUN dotnet restore "Services/UserService/UserService.Api/UserService.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/Services/UserService/UserService.Api"
RUN dotnet build "UserService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UserService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
```

---

## 8. Communication Patterns

### 8.1 Synchronous Communication (REST/HTTP)

| From Service | To Service | Purpose | Method |
|:-------------|:-----------|:--------|:-------|
| Order Service | User Service | Validate user | GET /api/v1/users/{id} |
| Order Service | Product Service | Get product details | GET /api/v1/products/{id} |
| Order Service | Inventory Service | Check stock | GET /api/v1/inventory/{sku} |
| Order Service | Payment Service | Process payment | POST /api/v1/payments |

### 8.2 Asynchronous Communication (RabbitMQ/SQS)

| Event | Publisher | Subscribers | Queue |
|:------|:----------|:------------|:------|
| `OrderCreated` | Order Service | Inventory, Notification | order.created |
| `PaymentCompleted` | Payment Service | Order, Notification | payment.completed |
| `InventoryReserved` | Inventory Service | Order | inventory.reserved |
| `UserRegistered` | User Service | Notification | user.registered |
| `ProductUpdated` | Product Service | Search Service | product.updated |

### 8.3 MassTransit Configuration

```csharp
// Program.cs - MassTransit setup
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ConfigureEndpoints(context);
    });
});
```

---

## 9. Evaluation Criteria and Scoring

### 9.1 Scoring Matrix - .NET 8

| Criteria | Weight | Score (1-10) | Weighted Score |
|:---------|:-------|:-------------|:---------------|
| Team Expertise | 25% | 10 | 2.5 |
| Performance | 15% | 9 | 1.35 |
| Microservices Support | 15% | 9 | 1.35 |
| AWS Integration | 15% | 9 | 1.35 |
| Developer Experience | 10% | 9 | 0.9 |
| Ecosystem | 10% | 9 | 0.9 |
| Long-term Support | 10% | 10 | 1.0 |
| **TOTAL** | **100%** | | **9.35/10** |

### 9.2 Scoring Matrix - Nginx

| Criteria | Weight | Score (1-10) | Weighted Score |
|:---------|:-------|:-------------|:---------------|
| Performance | 25% | 10 | 2.5 |
| Simplicity | 20% | 9 | 1.8 |
| Feature Set | 20% | 8 | 1.6 |
| AWS Integration | 15% | 8 | 1.2 |
| Documentation | 10% | 9 | 0.9 |
| Cost | 10% | 10 | 1.0 |
| **TOTAL** | **100%** | | **9.0/10** |

---

## 10. Benefits Analysis

### 10.1 Key Benefits - .NET 8

| ID | Benefit | Description | Impact |
|:---|:--------|:------------|:-------|
| B-01 | **Team Expertise** | Leverage existing .NET skills for faster development | High |
| B-02 | **Performance** | One of the fastest frameworks available | High |
| B-03 | **Tooling** | Visual Studio, Rider, excellent debugging | High |
| B-04 | **AWS SDK** | First-class AWS integration | High |
| B-05 | **Modern C#** | Records, pattern matching, minimal APIs | Medium |
| B-06 | **LTS Support** | 3-year support cycle | High |
| B-07 | **Container Ready** | Excellent Docker support, small images | Medium |

### 10.2 Key Benefits - Nginx

| ID | Benefit | Description | Impact |
|:---|:--------|:------------|:-------|
| B-08 | **Battle Tested** | Powers 30%+ of websites globally | High |
| B-09 | **Low Resource** | Minimal CPU/memory usage | Medium |
| B-10 | **Simple Config** | Easy to understand and maintain | Medium |
| B-11 | **Free** | No licensing costs | Medium |

---

## 11. Risks and Mitigations

### 11.1 Risk Assessment

| ID | Risk | Probability | Impact | Risk Level | Mitigation Strategy |
|:---|:-----|:------------|:-------|:-----------|:--------------------|
| R-01 | AWS service costs exceed budget | Medium | Medium | Medium | Use reserved instances, monitor costs |
| R-02 | Nginx config complexity grows | Low | Low | Low | Use templates, automate with Terraform |
| R-03 | Inter-service communication latency | Medium | Medium | Medium | Use async messaging, caching |
| R-04 | Single point of failure (Nginx) | Medium | High | High | Multiple Nginx instances, ALB |
| R-05 | Database connection exhaustion | Low | High | Medium | Connection pooling, read replicas |

---

## 12. Cost Analysis (AWS)

### 12.1 Monthly Infrastructure Costs (Estimated)

| Service | Configuration | Estimated Cost |
|:--------|:--------------|:---------------|
| **EKS** | 1 cluster | $73/month |
| **EC2 (Worker Nodes)** | 3x t3.medium | $100/month |
| **RDS PostgreSQL** | db.t3.medium, Multi-AZ | $150/month |
| **ElastiCache Redis** | cache.t3.micro, 2 nodes | $50/month |
| **OpenSearch** | t3.small.search, 2 nodes | $100/month |
| **DocumentDB** | db.t3.medium | $120/month |
| **S3** | 100GB storage | $5/month |
| **CloudFront** | 100GB transfer | $10/month |
| **ALB** | 1 load balancer | $25/month |
| **Route 53** | 1 hosted zone | $1/month |
| **Secrets Manager** | 10 secrets | $5/month |
| **CloudWatch** | Basic monitoring | $20/month |
| **Data Transfer** | 500GB | $45/month |
| **TOTAL** | | **~$704/month** |

### 12.2 Cost Optimization Strategies

| Strategy | Savings |
|:---------|:--------|
| Reserved Instances (1-year) | 30-40% |
| Spot Instances for non-critical | 60-90% |
| Right-sizing instances | 20-30% |
| S3 Intelligent-Tiering | 10-30% |

---

## 13. Implementation Timeline

| Phase | Duration | Deliverables |
|:------|:---------|:-------------|
| Phase 1 | Week 1-2 | AWS infrastructure (Terraform), EKS, Nginx |
| Phase 2 | Week 3-5 | User, Product, Cart Services |
| Phase 3 | Week 6-8 | Order, Payment, Inventory Services |
| Phase 4 | Week 9-10 | Search, Notification, Review Services |
| Phase 5 | Week 11-12 | Integration testing, Security audit |
| Phase 6 | Week 13-14 | Performance tuning, Production deployment |

---

## 14. Approval Signatures

### 14.1 Document Approval

| Role | Name | Signature | Date |
|:-----|:-----|:----------|:-----|
| **Technical Lead** | | | |
| **Solution Architect** | | | |
| **Project Manager** | | | |
| **Client Representative** | | | |

---

## 15. Appendices

### Appendix A: .NET 8 vs Spring Boot Comparison

| Feature | .NET 8 | Spring Boot 3 |
|:--------|:-------|:--------------|
| Language | C# 12 | Java 17+ |
| Performance | Excellent | Very Good |
| Memory Usage | Lower | Higher |
| Startup Time | Fast | Slower |
| Native AOT | Yes | Yes (GraalVM) |
| Container Size | ~100MB | ~200MB |
| Team Expertise | Strong | None |

### Appendix B: Reference Links

| Resource | URL |
|:---------|:----|
| .NET Documentation | https://docs.microsoft.com/dotnet |
| ASP.NET Core | https://docs.microsoft.com/aspnet/core |
| Nginx Documentation | https://nginx.org/en/docs |
| AWS .NET SDK | https://aws.amazon.com/sdk-for-net |
| EKS Best Practices | https://aws.github.io/aws-eks-best-practices |

### Appendix C: Revision History

| Version | Date | Author | Changes |
|:--------|:-----|:-------|:--------|
| 1.0 | Dec 19, 2024 | Technical Team | Initial document (Spring Boot) |
| 1.1 | Dec 19, 2024 | Technical Team | Updated to .NET 8 + Nginx + AWS |

---

**END OF DOCUMENT**

