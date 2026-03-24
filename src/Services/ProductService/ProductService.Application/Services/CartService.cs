using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;
    private readonly ICouponRepository _coupons;

    public CartService(ICartRepository carts, IProductRepository products, ICouponRepository coupons)
    {
        _carts = carts;
        _products = products;
        _coupons = coupons;
    }

    public async Task<CartDto> GetCartAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(userId, sessionId, ct);
        return await BuildCartDtoAsync(cart.Id, ct);
    }

    public async Task<CartDto> AddItemAsync(Guid? userId, string? sessionId, Guid productId, int quantity, CancellationToken ct = default)
    {
        if (quantity < 1)
            throw new InvalidOperationException("Quantity must be at least 1.");

        var product = await _products.GetByIdAsync(productId, publicOnly: true, ct);
        if (product == null)
            throw new InvalidOperationException("Product not found or is not available for the storefront.");

        var cart = await _carts.GetOrCreateAsync(userId, sessionId, ct);
        var unitPrice = product.Price;

        var existing = await _carts.FindItemAsync(cart.Id, productId, ct);
        var newQty = (existing?.Quantity ?? 0) + quantity;
        EnsureStock(product, newQty);

        if (existing != null)
        {
            existing.Quantity = newQty;
            existing.UnitPrice = unitPrice;
            existing.Subtotal = newQty * unitPrice;
            await _carts.UpdateCartItemAsync(existing, ct);
        }
        else
        {
            await _carts.AddCartItemAsync(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = quantity * unitPrice
            }, ct);
        }

        return await BuildCartDtoAsync(cart.Id, ct);
    }

    public async Task<CartDto> UpdateItemAsync(Guid? userId, string? sessionId, Guid productId, int quantity, CancellationToken ct = default)
    {
        if (quantity < 1)
            throw new InvalidOperationException("Quantity must be at least 1. Remove the line to delete.");

        var cart = await _carts.GetOrCreateAsync(userId, sessionId, ct);
        var item = await _carts.FindItemAsync(cart.Id, productId, ct);
        if (item == null)
            throw new InvalidOperationException("Item is not in the cart.");

        var product = await _products.GetByIdAsync(productId, publicOnly: true, ct);
        if (product == null)
            throw new InvalidOperationException("Product is no longer available.");

        EnsureStock(product, quantity);
        var unitPrice = product.Price;
        item.Quantity = quantity;
        item.UnitPrice = unitPrice;
        item.Subtotal = quantity * unitPrice;
        await _carts.UpdateCartItemAsync(item, ct);

        return await BuildCartDtoAsync(cart.Id, ct);
    }

    public async Task<CartDto> RemoveItemAsync(Guid? userId, string? sessionId, Guid productId, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(userId, sessionId, ct);
        var item = await _carts.FindItemAsync(cart.Id, productId, ct);
        if (item == null)
            return await BuildCartDtoAsync(cart.Id, ct);

        await _carts.RemoveItemAsync(item, ct);
        return await BuildCartDtoAsync(cart.Id, ct);
    }

    public async Task<CartDto> ApplyCouponAsync(Guid? userId, string? sessionId, string code, CancellationToken ct = default)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(normalized))
            throw new InvalidOperationException("Coupon code is required.");

        var coupon = await _coupons.GetByCodeAsync(normalized, ct);
        if (coupon == null)
            throw new InvalidOperationException("Invalid or inactive coupon code.");

        var cart = await _carts.GetOrCreateAsync(userId, sessionId, ct);
        var dto = await BuildCartDtoAsync(cart.Id, ct);
        var now = DateTime.UtcNow;
        if (now < coupon.ValidFrom || now > coupon.ValidTo || !coupon.IsActive)
            throw new InvalidOperationException("This coupon is not valid at this time.");

        if (coupon.MinOrderTotal.HasValue && dto.Subtotal < coupon.MinOrderTotal.Value)
            throw new InvalidOperationException($"Order subtotal must be at least {coupon.MinOrderTotal.Value} to use this coupon.");

        await _carts.SetCouponAsync(cart.Id, coupon.Id, ct);
        return await BuildCartDtoAsync(cart.Id, ct);
    }

    public async Task<CartDto> RemoveCouponAsync(Guid? userId, string? sessionId, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(userId, sessionId, ct);
        await _carts.SetCouponAsync(cart.Id, null, ct);
        return await BuildCartDtoAsync(cart.Id, ct);
    }

    public async Task MergeGuestCartAsync(Guid userId, string sessionId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        var guest = await _carts.GetBySessionIdWithItemsAsync(sessionId.Trim(), ct);
        if (guest == null || guest.Items.Count == 0)
            return;

        var userCart = await _carts.GetOrCreateAsync(userId, null, ct);

        foreach (var gi in guest.Items.ToList())
        {
            var product = await _products.GetByIdAsync(gi.ProductId, publicOnly: true, ct);
            if (product == null)
                continue;

            var unit = product.Price;
            var existing = await _carts.FindItemAsync(userCart.Id, gi.ProductId, ct);
            var mergedQty = (existing?.Quantity ?? 0) + gi.Quantity;
            EnsureStock(product, mergedQty);

            if (existing != null)
            {
                existing.Quantity = mergedQty;
                existing.UnitPrice = unit;
                existing.Subtotal = mergedQty * unit;
                await _carts.UpdateCartItemAsync(existing, ct);
            }
            else
            {
                await _carts.AddCartItemAsync(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = userCart.Id,
                    ProductId = gi.ProductId,
                    Quantity = gi.Quantity,
                    UnitPrice = unit,
                    Subtotal = gi.Quantity * unit
                }, ct);
            }
        }

        var userMeta = await _carts.GetByIdAsync(userCart.Id, ct);
        if (userMeta?.AppliedCouponId == null && guest.AppliedCouponId != null)
        {
            var c = await _coupons.GetByIdAsync(guest.AppliedCouponId.Value, ct);
            if (c != null && c.IsActive)
                await _carts.SetCouponAsync(userCart.Id, guest.AppliedCouponId, ct);
        }

        await _carts.DeleteCartAsync(guest, ct);
    }

    private static void EnsureStock(Product product, int requestedQty)
    {
        if (!product.TrackInventory)
            return;
        if (requestedQty > product.Quantity)
            throw new InvalidOperationException($"Only {product.Quantity} unit(s) in stock for this product.");
    }

    private async Task<CartDto> BuildCartDtoAsync(Guid cartId, CancellationToken ct)
    {
        var cartMeta = await _carts.GetByIdAsync(cartId, ct);
        if (cartMeta == null)
            return new CartDto { CartId = cartId };

        var rows = await _carts.ListItemsWithProductsAsync(cartId, ct);
        var lines = new List<CartLineDto>();
        decimal subtotal = 0;
        var currency = "USD";

        foreach (var row in rows)
        {
            var p = row.Product;
            var available = p != null && p.Status == "active";
            var unitPrice = available ? p!.Price : 0m;
            var compareAt = available ? p!.CompareAtPrice : null;
            if (available && p!.Currency != null)
                currency = p.Currency;

            var lineTotal = unitPrice * row.Quantity;
            subtotal += lineTotal;

            var primary = p?.Images?.FirstOrDefault(i => i.IsPrimary)
                          ?? p?.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault();

            lines.Add(new CartLineDto
            {
                CartItemId = row.Id,
                ProductId = row.ProductId,
                ProductName = p?.Name,
                ProductSlug = p?.Slug,
                PrimaryImageUrl = primary?.Url,
                Quantity = row.Quantity,
                UnitPrice = unitPrice,
                CompareAtPrice = compareAt,
                LineSubtotal = lineTotal,
                Currency = p?.Currency ?? currency,
                ProductAvailable = available
            });
        }

        Coupon? coupon = null;
        if (cartMeta.AppliedCouponId != null)
            coupon = await _coupons.GetByIdAsync(cartMeta.AppliedCouponId.Value, ct);

        var now = DateTime.UtcNow;
        var couponOk = coupon != null && coupon.IsActive && now >= coupon.ValidFrom && now <= coupon.ValidTo;
        if (couponOk && coupon!.MinOrderTotal.HasValue && subtotal < coupon.MinOrderTotal.Value)
            couponOk = false;

        if (cartMeta.AppliedCouponId != null && !couponOk)
        {
            await _carts.SetCouponAsync(cartId, null, ct);
            coupon = null;
            couponOk = false;
        }

        var discount = couponOk && coupon != null ? ComputeDiscount(subtotal, coupon) : 0m;
        var total = Math.Max(0, subtotal - discount);

        return new CartDto
        {
            CartId = cartId,
            Items = lines,
            Subtotal = subtotal,
            CouponCode = couponOk ? coupon!.Code : null,
            DiscountAmount = discount,
            Total = total,
            Currency = currency
        };
    }

    private static decimal ComputeDiscount(decimal subtotal, Coupon coupon)
    {
        if (string.Equals(coupon.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
            return Math.Round(subtotal * (coupon.DiscountValue / 100m), 2, MidpointRounding.AwayFromZero);
        if (string.Equals(coupon.DiscountType, "FixedAmount", StringComparison.OrdinalIgnoreCase))
            return Math.Min(coupon.DiscountValue, subtotal);
        return 0;
    }
}
