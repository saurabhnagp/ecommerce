namespace AmCart.ProductService.Application.Common;

/// <summary>Optional storefront / admin inventory filters for paged product queries.</summary>
public enum ProductStockFilter
{
    None = 0,
    /// <summary>Tracked inventory and quantity &lt; 1 (public catalog: unavailable items).</summary>
    OutOfStock = 1,
    /// <summary>Tracked, quantity between 1 and the product&apos;s low-stock threshold (admin only).</summary>
    LowStock = 2,
}
