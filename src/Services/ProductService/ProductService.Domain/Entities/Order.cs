namespace AmCart.ProductService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid? UserId { get; set; }

    public string PaymentMethod { get; set; } = null!;
    public string Currency { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal Total { get; set; }
    public string? CouponCode { get; set; }

    /// <summary>Fulfillment status (e.g. Placed, Processing, Shipped, Delivered).</summary>
    public string Status { get; set; } = "Placed";

    public DateTime? ShippedAt { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }

    public string BillFirstName { get; set; } = null!;
    public string BillLastName { get; set; } = null!;
    public string? BillCompany { get; set; }
    public string BillCountry { get; set; } = null!;
    public string BillStreet { get; set; } = null!;
    public string BillApartment { get; set; } = null!;
    public string BillCity { get; set; } = null!;
    public string BillState { get; set; } = null!;
    public string BillZip { get; set; } = null!;
    public string BillPhone { get; set; } = null!;
    public string BillEmail { get; set; } = null!;

    public string ShipFirstName { get; set; } = null!;
    public string ShipLastName { get; set; } = null!;
    public string? ShipCompany { get; set; }
    public string ShipCountry { get; set; } = null!;
    public string ShipStreet { get; set; } = null!;
    public string ShipApartment { get; set; } = null!;
    public string ShipCity { get; set; } = null!;
    public string ShipState { get; set; } = null!;
    public string ShipZip { get; set; } = null!;
    public string ShipPhone { get; set; } = null!;
    public string ShipEmail { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
