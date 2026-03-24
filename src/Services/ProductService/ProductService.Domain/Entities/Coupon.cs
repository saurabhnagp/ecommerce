namespace AmCart.ProductService.Domain.Entities;

/// <summary>Percentage or fixed-amount discount; code applied on cart.</summary>
public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    /// <summary>"Percentage" or "FixedAmount".</summary>
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderTotal { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
}
