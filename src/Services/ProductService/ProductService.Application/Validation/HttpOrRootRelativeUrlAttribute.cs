using System.ComponentModel.DataAnnotations;

namespace AmCart.ProductService.Application.Validation;

/// <summary>
/// Allows absolute http(s) URLs or site-root-relative paths (e.g. /product-images/x.svg).
/// Rejects protocol-relative URLs (//...) and non-http schemes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HttpOrRootRelativeUrlAttribute : ValidationAttribute
{
    public HttpOrRootRelativeUrlAttribute()
        : base("The {0} field must be an http(s) URL or a path starting with \"/\" (not \"//\").")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string s)
            return ValidationResult.Success;

        var t = s.Trim();
        if (t.Length == 0)
            return ValidationResult.Success;

        if (t.Length > 500)
            return new ValidationResult("The field must not exceed 500 characters.");

        if (t.StartsWith('/'))
        {
            if (t.StartsWith("//", StringComparison.Ordinal))
                return new ValidationResult("Protocol-relative URLs are not allowed.");
            return ValidationResult.Success;
        }

        if (!Uri.TryCreate(t, UriKind.Absolute, out var uri))
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        return ValidationResult.Success;
    }
}
