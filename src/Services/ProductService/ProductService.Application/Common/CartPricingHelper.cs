using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Common;

public static class CartPricingHelper
{
    public static decimal ComputeDiscount(decimal subtotal, Coupon? coupon, DateTime utcNow)
    {
        if (coupon == null)
            return 0m;
        if (!coupon.IsActive || utcNow < coupon.ValidFrom || utcNow > coupon.ValidTo)
            return 0m;
        if (coupon.MinOrderTotal.HasValue && subtotal < coupon.MinOrderTotal.Value)
            return 0m;
        if (string.Equals(coupon.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
            return Math.Round(subtotal * (coupon.DiscountValue / 100m), 2, MidpointRounding.AwayFromZero);
        if (string.Equals(coupon.DiscountType, "FixedAmount", StringComparison.OrdinalIgnoreCase))
            return Math.Min(coupon.DiscountValue, subtotal);
        return 0m;
    }
}
