namespace AmCart.ProductService.Application.DTOs;

public class CheckoutAddressDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Company { get; set; }
    public string Country { get; set; } = "";
    public string Street { get; set; } = "";
    public string Apartment { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
}

public class PlaceOrderRequest
{
    public CheckoutAddressDto Billing { get; set; } = new();
    public bool SameAsBilling { get; set; } = true;
    public CheckoutAddressDto? Shipping { get; set; }
    /// <summary>DirectBankTransfer, Cheque, or PayPal</summary>
    public string PaymentMethod { get; set; } = "";
}

public class OrderLineConfirmationDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string Currency { get; set; } = "USD";
}

public class OrderConfirmationDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string Currency { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal Total { get; set; }
    public string? CouponCode { get; set; }
    public string Status { get; set; } = "Placed";
    public DateTime? ShippedAt { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public CheckoutAddressDto Billing { get; set; } = new();
    public CheckoutAddressDto Shipping { get; set; } = new();
    public IReadOnlyList<OrderLineConfirmationDto> Items { get; set; } = Array.Empty<OrderLineConfirmationDto>();
}
