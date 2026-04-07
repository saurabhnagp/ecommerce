using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Common;

public static class OrderMapping
{
    public static CheckoutAddressDto ToBillingDto(Order o) =>
        new()
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

    public static CheckoutAddressDto ToShippingDto(Order o) =>
        new()
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

    public static OrderLineConfirmationDto ToLineDto(OrderItem i) =>
        new()
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            LineTotal = i.LineTotal,
            Currency = i.Currency,
        };

    public static OrderConfirmationDto ToConfirmationDto(Order o) =>
        new()
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            CreatedAt = o.CreatedAt,
            PaymentMethod = o.PaymentMethod,
            Currency = o.Currency,
            Subtotal = o.Subtotal,
            DiscountAmount = o.DiscountAmount,
            ShippingAmount = o.ShippingAmount,
            Total = o.Total,
            CouponCode = o.CouponCode,
            Status = o.Status,
            ShippedAt = o.ShippedAt,
            Carrier = o.Carrier,
            TrackingNumber = o.TrackingNumber,
            Billing = ToBillingDto(o),
            Shipping = ToShippingDto(o),
            Items = o.Items.OrderBy(x => x.Id).Select(ToLineDto).ToList(),
        };

    public static string? PrimaryImageUrl(Product? p)
    {
        if (p?.Images == null || p.Images.Count == 0)
            return null;
        var primary = p.Images.FirstOrDefault(i => i.IsPrimary)
                      ?? p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault();
        return primary?.Url;
    }

    public static OrderHistoryItemDto ToHistoryItemDto(Order o)
    {
        var first = o.Items.OrderBy(i => i.Id).FirstOrDefault();
        var previewName = first?.ProductName;
        var previewUrl = first != null ? PrimaryImageUrl(first.Product) : null;

        return new OrderHistoryItemDto
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            CreatedAt = o.CreatedAt,
            Status = o.Status,
            ShippedAt = o.ShippedAt,
            Total = o.Total,
            Currency = o.Currency,
            PreviewProductName = previewName,
            PreviewImageUrl = previewUrl,
            LineItemCount = o.Items.Count,
        };
    }
}
