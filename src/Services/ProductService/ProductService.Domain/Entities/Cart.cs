namespace AmCart.ProductService.Domain.Entities;

/// <summary>One cart per user (logged-in) or per guest session id.</summary>
public class Cart
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public Guid? AppliedCouponId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Coupon? AppliedCoupon { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
