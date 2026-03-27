using System.Text.RegularExpressions;
using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Application.Interfaces;

namespace AmCart.ProductService.Application.Services;

public partial class CheckoutService : ICheckoutService
{
    private static readonly Regex EmailRegex = EmailRegexImpl();
    private static readonly Regex ZipRegex = ZipRegexImpl();

    private readonly ICartService _cart;
    private readonly ICheckoutRepository _checkout;

    public CheckoutService(ICartService cart, ICheckoutRepository checkout)
    {
        _cart = cart;
        _checkout = checkout;
    }

    public async Task<OrderConfirmationDto> PlaceOrderAsync(
        Guid? userId,
        string? sessionId,
        PlaceOrderRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Billing);

        ValidateAddress(request.Billing, "Billing");
        if (!request.SameAsBilling)
        {
            if (request.Shipping == null)
                throw new InvalidOperationException("Shipping address is required when not same as billing.");
            ValidateAddress(request.Shipping, "Shipping");
        }

        var payment = NormalizePaymentMethod(request.PaymentMethod);
        if (payment == null)
            throw new InvalidOperationException(
                "Select a payment method: Direct Bank Transfer, Cheque, or PayPal.");

        request.PaymentMethod = payment;

        var cart = await _cart.GetCartAsync(userId, sessionId, ct);
        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Your cart is empty.");
        if (cart.Items.Any(l => !l.ProductAvailable))
            throw new InvalidOperationException("Remove unavailable items from your cart before checkout.");

        return await _checkout.PlaceOrderAsync(cart.CartId, userId, sessionId, request, ct);
    }

    private static void ValidateAddress(CheckoutAddressDto a, string label)
    {
        void Req(string field, string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"{label}: {name} is required.");
        }

        Req("fn", a.FirstName, "First name");
        Req("ln", a.LastName, "Last name");
        // Company optional
        Req("ct", a.Country, "Country");
        Req("st", a.Street, "Street address");
        Req("apt", a.Apartment, "Apartment / suite");
        Req("city", a.City, "City");
        Req("state", a.State, "State / province");
        Req("zip", a.Zip, "ZIP / postal code");
        Req("ph", a.Phone, "Phone");
        Req("em", a.Email, "Email");

        var email = a.Email.Trim();
        if (!EmailRegex.IsMatch(email))
            throw new InvalidOperationException($"{label}: Enter a valid email address.");

        var zip = a.Zip.Trim();
        if (!ZipRegex.IsMatch(zip))
            throw new InvalidOperationException(
                $"{label}: ZIP / postal code must be 3–16 characters (letters, numbers, spaces, hyphens).");
    }

    private static string? NormalizePaymentMethod(string? raw)
    {
        var s = (raw ?? "").Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "");
        return s switch
        {
            "directbanktransfer" => "DirectBankTransfer",
            "cheque" or "check" => "Cheque",
            "paypal" => "PayPal",
            _ => null,
        };
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegexImpl();

    [GeneratedRegex(@"^[\p{L}\p{N}][\p{L}\p{N}\s\-]{1,14}[\p{L}\p{N}]$|^[\p{L}\p{N}]{3,16}$", RegexOptions.CultureInvariant)]
    private static partial Regex ZipRegexImpl();
}
