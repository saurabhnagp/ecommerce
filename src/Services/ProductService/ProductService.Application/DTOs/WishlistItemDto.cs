namespace AmCart.ProductService.Application.DTOs;

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateTime AddedAt { get; set; }

    public string? ProductName { get; set; }
    public string? ProductSlug { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? ProductStatus { get; set; }

    /// <summary>False when product was removed from catalog (soft-delete) or not loaded.</summary>
    public bool ProductLoaded { get; set; }
}
