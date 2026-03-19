# ADR-007: Event-Driven Architecture

## Status
**Accepted**

## Date
2024-12-19

## Context

In our microservices architecture, services need to communicate for:
- Order placement (inventory reservation, payment, notifications)
- Payment completion (order confirmation, email)
- Product updates (search index sync)
- User registration (welcome email)

Synchronous HTTP calls between services create:
- Tight coupling
- Cascading failures
- Latency accumulation
- Scalability bottlenecks

## Decision

We will implement **event-driven architecture** using:
- **RabbitMQ** for message brokering (self-hosted or Amazon MQ)
- **MassTransit** as the .NET abstraction layer
- **Outbox Pattern** for reliable message delivery

### Event Categories

| Category | Pattern | Example |
|----------|---------|---------|
| Domain Events | Publish/Subscribe | OrderCreated, PaymentCompleted |
| Integration Events | Publish/Subscribe | ProductUpdated (for search sync) |
| Commands | Point-to-Point | SendEmail, ProcessPayment |

### Key Events

```
┌─────────────────────────────────────────────────────────────┐
│                      Event Flow                              │
│                                                              │
│  Order Service                                               │
│       │                                                      │
│       ├──▶ OrderCreated                                     │
│       │         │                                            │
│       │         ├──▶ Inventory Service (Reserve Stock)      │
│       │         ├──▶ Notification Service (Order Email)     │
│       │         └──▶ Payment Service (Await Payment)        │
│       │                                                      │
│  Payment Service                                             │
│       │                                                      │
│       ├──▶ PaymentCompleted                                 │
│       │         │                                            │
│       │         ├──▶ Order Service (Confirm Order)          │
│       │         ├──▶ Notification Service (Receipt Email)   │
│       │         └──▶ Inventory Service (Confirm Reserve)    │
│       │                                                      │
│       └──▶ PaymentFailed                                    │
│                 │                                            │
│                 ├──▶ Order Service (Cancel Order)           │
│                 ├──▶ Inventory Service (Release Stock)      │
│                 └──▶ Notification Service (Failure Email)   │
│                                                              │
│  Product Service                                             │
│       │                                                      │
│       └──▶ ProductUpdated                                   │
│                 │                                            │
│                 └──▶ Search Service (Update Index)          │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Consequences

### Positive

- **Loose Coupling**: Services don't need to know about each other
- **Resilience**: Failures don't cascade immediately
- **Scalability**: Consumers can scale independently
- **Flexibility**: Easy to add new consumers
- **Auditability**: Events provide audit trail
- **Temporal Decoupling**: Producer and consumer don't need simultaneous availability

### Negative

- **Complexity**: Harder to trace request flow
- **Eventual Consistency**: Data may be stale temporarily
- **Message Ordering**: May need special handling
- **Debugging**: Distributed tracing required
- **Infrastructure**: Message broker adds operational overhead

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Complexity | Clear event naming, documentation |
| Eventual Consistency | Design for it, inform users |
| Message Ordering | Partition by aggregate ID |
| Debugging | Distributed tracing (Zipkin/X-Ray) |
| Infrastructure | AWS managed services |

## Implementation

### Event Definition

```csharp
// EventBus.Messages/Events/OrderCreatedEvent.cs
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; }
    public List<OrderItemEvent> Items { get; init; }
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record OrderItemEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public string Sku { get; init; }
    public int Quantity { get; init; }
}
```

### Publisher

```csharp
// Order Service - Publishing event
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IOrderRepository _orderRepository;

    public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Create order
        var order = new Order(request.UserId, request.Items);
        await _orderRepository.AddAsync(order, ct);

        // Publish event
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            Items = order.Items.Select(i => new OrderItemEvent
            {
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                Sku = i.Sku,
                Quantity = i.Quantity
            }).ToList(),
            Total = order.Total,
            CreatedAt = DateTime.UtcNow
        }, ct);

        return new OrderResult(order);
    }
}
```

### Consumer

```csharp
// Inventory Service - Consuming event
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Reserving inventory for order {OrderId}", message.OrderId);

        foreach (var item in message.Items)
        {
            await _inventoryService.ReserveStockAsync(
                item.VariantId, 
                item.Quantity,
                message.OrderId);
        }

        _logger.LogInformation("Inventory reserved for order {OrderId}", message.OrderId);
    }
}
```

### MassTransit Configuration

```csharp
// Program.cs
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ConfigureEndpoints(context);

        // Retry policy
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromMinutes(1),
            intervalDelta: TimeSpan.FromSeconds(5)));
    });
});
```

## Saga Pattern for Complex Workflows

For the order workflow, we use the **Saga Pattern**:

```
┌───────────────────────────────────────────────────────────┐
│                    Order Saga                              │
│                                                            │
│  1. CreateOrder                                            │
│       │                                                    │
│       ▼                                                    │
│  2. ReserveInventory ─────┐                               │
│       │                   │ (failure)                      │
│       ▼                   ▼                                │
│  3. ProcessPayment    CancelOrder                          │
│       │                                                    │
│       ├───────────┐                                        │
│       │ (success) │ (failure)                              │
│       ▼           ▼                                        │
│  4. ConfirmOrder  ReleaseInventory                         │
│       │               │                                    │
│       ▼               ▼                                    │
│  5. SendConfirmation  CancelOrder                          │
│                                                            │
└───────────────────────────────────────────────────────────┘
```

## Alternatives Considered

### 1. Direct HTTP Calls

**Pros:**
- Simpler to implement
- Immediate consistency
- Easier debugging

**Cons:**
- Tight coupling
- Cascading failures
- Synchronous bottlenecks

**Why Rejected:** Creates fragile system with cascading failures.

### 2. AWS SQS/SNS

**Pros:**
- Fully managed
- Pay per use
- No infrastructure

**Cons:**
- AWS lock-in
- Less feature-rich than RabbitMQ
- Different programming model

**Why Considered:** May migrate to SQS for production if needed.

### 3. Apache Kafka

**Pros:**
- Event sourcing capable
- High throughput
- Replay capability

**Cons:**
- More complex
- Overkill for current scale
- Higher operational overhead

**Why Rejected:** Complexity not justified for current requirements.

## References

- [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/)
- [MassTransit Documentation](https://masstransit-project.com/)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)

