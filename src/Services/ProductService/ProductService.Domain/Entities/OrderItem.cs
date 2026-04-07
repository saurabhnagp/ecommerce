namespace AmCart.ProductService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string Currency { get; set; } = "USD";

    public Order Order { get; set; } = null!;
    public Product? Product { get; set; }
}
