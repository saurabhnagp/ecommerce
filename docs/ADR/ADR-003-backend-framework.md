# ADR-003: Backend Framework Selection

## Status
**Accepted**

## Date
2024-12-19

## Context

AmCart requires backend services that support:
- RESTful API development
- High performance for transaction processing
- Database connectivity (PostgreSQL, MongoDB, Redis, Elasticsearch)
- Message queue integration (RabbitMQ)
- JWT authentication
- Docker containerization
- AWS deployment

The development team has:
- Strong experience in .NET ecosystem
- Limited experience in Java/Node.js
- Need for rapid development with familiar tools

## Decision

We will use **.NET 8** with ASP.NET Core for all backend microservices.

### Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8 |
| Web API | ASP.NET Core |
| Language | C# 12 |
| ORM | Entity Framework Core 8 |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| CQRS | MediatR |
| Resilience | Polly |
| Messaging | MassTransit |
| Logging | Serilog |
| Documentation | Swashbuckle (Swagger) |

### Architecture Pattern

Clean Architecture with CQRS:

```
Service/
├── Api/           # Controllers, middleware, DI
├── Application/   # Commands, queries, handlers, DTOs
├── Domain/        # Entities, interfaces, events
└── Infrastructure/ # Repositories, external services
```

## Consequences

### Positive

- **Team Expertise**: Leverages existing .NET skills
- **High Performance**: .NET 8 is one of the fastest frameworks
- **Excellent Tooling**: Visual Studio, Rider, great debugging
- **AWS SDK**: First-class AWS integration
- **Type Safety**: Strong typing catches errors at compile time
- **LTS Support**: 3-year support cycle
- **Container Ready**: Small Docker images (~100MB)

### Negative

- **Memory Usage**: Higher than Go, comparable to Java
- **Startup Time**: Slower than Go (mitigated by Native AOT)
- **Windows Perception**: Some perceive .NET as Windows-only (no longer true)

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Memory | Proper resource limits, horizontal scaling |
| Startup | Use Native AOT for critical services |
| Perception | Deploy on Linux containers, leverage cross-platform |

## Alternatives Considered

### 1. Java Spring Boot

**Pros:**
- Mature microservices ecosystem (Spring Cloud)
- Large enterprise adoption
- Extensive documentation

**Cons:**
- Team lacks Java experience
- Higher memory usage
- Slower startup time

**Why Rejected:** Team expertise is in .NET, not Java.

### 2. Node.js (NestJS)

**Pros:**
- Same language as frontend (TypeScript)
- Fast development
- Good microservices support

**Cons:**
- Single-threaded limitations
- Callback/async complexity
- Less suitable for CPU-intensive tasks

**Why Rejected:** Team prefers strongly-typed compiled languages.

### 3. Go (Golang)

**Pros:**
- Excellent performance
- Low memory usage
- Fast compilation

**Cons:**
- Team has no Go experience
- Smaller ecosystem
- More verbose error handling

**Why Rejected:** Learning curve and team expertise.

## Implementation Notes

### Project Structure (Per Service)

```
UserService/
├── UserService.Api/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Extensions/
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── UserService.Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   ├── Validators/
│   └── Mappings/
├── UserService.Domain/
│   ├── Entities/
│   ├── Interfaces/
│   ├── Enums/
│   └── Events/
└── UserService.Infrastructure/
    ├── Data/
    ├── Repositories/
    └── Services/
```

### Sample Controller

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var result = await _mediator.Send(new GetUserQuery(id));
        return result.Match<ActionResult>(
            success => Ok(success),
            notFound => NotFound());
    }
}
```

## References

- [.NET Documentation](https://docs.microsoft.com/dotnet)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR](https://github.com/jbogard/MediatR)

