using AmCart.ProductService.Application.Common;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Domain.Entities;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Repositories;

public class CheckoutRepository : ICheckoutRepository
{
    private readonly ProductDbContext _db;
    private readonly ICouponRepository _coupons;

    public CheckoutRepository(ProductDbContext db, ICouponRepository coupons)
    {
        _db = db;
        _coupons = coupons;
    }

    public async Task<OrderConfirmationDto> PlaceOrderAsync(
        Guid cartId,
        Guid? cartUserId,
        string? cartSessionId,
        PlaceOrderRequest request,
        CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId, ct);

            if (cart == null)
                throw new InvalidOperationException("Cart not found.");

            var owns = (cartUserId != null && cart.UserId == cartUserId)
                       || (cartUserId == null && !string.IsNullOrWhiteSpace(cartSessionId)
                           && string.Equals(cart.SessionId, cartSessionId.Trim(), StringComparison.Ordinal));
            if (!owns)
                throw new InvalidOperationException("Cart does not match the current session.");

            if (cart.Items.Count == 0)
                throw new InvalidOperationException("Your cart is empty.");

            decimal subtotal = 0;
            var currency = "USD";
            var now = DateTime.UtcNow;

            foreach (var line in cart.Items)
            {
                var p = line.Product;
                if (p == null || !string.Equals(p.Status, "active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("A product in your cart is no longer available.");
                if (p.TrackInventory && p.Quantity < line.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for \"{p.Name}\". Only {p.Quantity} unit(s) available.");

                var unit = p.Price;
                subtotal += unit * line.Quantity;
                if (!string.IsNullOrEmpty(p.Currency))
                    currency = p.Currency;
            }

            Coupon? coupon = null;
            if (cart.AppliedCouponId != null)
                coupon = await _coupons.GetByIdAsync(cart.AppliedCouponId.Value, ct);

            var discount = CartPricingHelper.ComputeDiscount(subtotal, coupon, now);
            var couponCode = discount > 0 && coupon != null ? coupon.Code : null;
            const decimal shipping = 0m;
            var total = Math.Max(0, subtotal - discount + shipping);

            var ship = request.SameAsBilling || request.Shipping == null
                ? request.Billing
                : request.Shipping;

            var orderNumber = $"AC-{now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                Id = orderId,
                OrderNumber = orderNumber,
                CreatedAt = now,
                UserId = cart.UserId,
                PaymentMethod = request.PaymentMethod,
                Currency = currency,
                Subtotal = subtotal,
                DiscountAmount = discount,
                ShippingAmount = shipping,
                Total = total,
                CouponCode = couponCode,
                Status = "Placed",
                ShippedAt = null,
                Carrier = null,
                TrackingNumber = null,
                BillFirstName = request.Billing.FirstName.Trim(),
                BillLastName = request.Billing.LastName.Trim(),
                BillCompany = string.IsNullOrWhiteSpace(request.Billing.Company) ? null : request.Billing.Company.Trim(),
                BillCountry = request.Billing.Country.Trim(),
                BillStreet = request.Billing.Street.Trim(),
                BillApartment = request.Billing.Apartment.Trim(),
                BillCity = request.Billing.City.Trim(),
                BillState = request.Billing.State.Trim(),
                BillZip = request.Billing.Zip.Trim(),
                BillPhone = request.Billing.Phone.Trim(),
                BillEmail = request.Billing.Email.Trim(),
                ShipFirstName = ship.FirstName.Trim(),
                ShipLastName = ship.LastName.Trim(),
                ShipCompany = string.IsNullOrWhiteSpace(ship.Company) ? null : ship.Company.Trim(),
                ShipCountry = ship.Country.Trim(),
                ShipStreet = ship.Street.Trim(),
                ShipApartment = ship.Apartment.Trim(),
                ShipCity = ship.City.Trim(),
                ShipState = ship.State.Trim(),
                ShipZip = ship.Zip.Trim(),
                ShipPhone = ship.Phone.Trim(),
                ShipEmail = ship.Email.Trim(),
            };

            var lineDtos = new List<OrderLineConfirmationDto>();

            foreach (var line in cart.Items)
            {
                var p = line.Product!;
                var unit = p.Price;
                var lineTotal = unit * line.Quantity;

                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = p.Id,
                    ProductName = p.Name,
                    UnitPrice = unit,
                    Quantity = line.Quantity,
                    LineTotal = lineTotal,
                    Currency = p.Currency ?? currency,
                });

                if (p.TrackInventory)
                    p.Quantity -= line.Quantity;

                lineDtos.Add(new OrderLineConfirmationDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Quantity = line.Quantity,
                    UnitPrice = unit,
                    LineTotal = lineTotal,
                    Currency = p.Currency ?? currency,
                });
            }

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.AppliedCouponId = null;
            cart.UpdatedAt = now;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OrderConfirmationDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CreatedAt = order.CreatedAt,
                PaymentMethod = order.PaymentMethod,
                Currency = order.Currency,
                Subtotal = order.Subtotal,
                DiscountAmount = order.DiscountAmount,
                ShippingAmount = order.ShippingAmount,
                Total = order.Total,
                CouponCode = order.CouponCode,
                Status = order.Status,
                ShippedAt = order.ShippedAt,
                Carrier = order.Carrier,
                TrackingNumber = order.TrackingNumber,
                Billing = ToAddressDto(order, billing: true),
                Shipping = ToAddressDto(order, billing: false),
                Items = lineDtos,
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static CheckoutAddressDto ToAddressDto(Order o, bool billing)
    {
        if (billing)
            return new CheckoutAddressDto
            {
                FirstName = o.BillFirstName,
                LastName = o.BillLastName,
                Company = o.BillCompany,
                Country = o.BillCountry,
                Street = o.BillStreet,
                Apartment = o.BillApartment,
                City = o.BillCity,
                State = o.BillState,
                Zip = o.BillZip,
                Phone = o.BillPhone,
                Email = o.BillEmail,
            };

        return new CheckoutAddressDto
        {
            FirstName = o.ShipFirstName,
            LastName = o.ShipLastName,
            Company = o.ShipCompany,
            Country = o.ShipCountry,
            Street = o.ShipStreet,
            Apartment = o.ShipApartment,
            City = o.ShipCity,
            State = o.ShipState,
            Zip = o.ShipZip,
            Phone = o.ShipPhone,
            Email = o.ShipEmail,
        };
    }
}
