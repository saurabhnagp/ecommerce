namespace AmCart.ProductService.Domain.Entities;

public class WishlistItem
{
    public Guid Id { get; set; }
    public Guid WishlistId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime AddedAt { get; set; }

    public Wishlist Wishlist { get; set; } = null!;
    public Product? Product { get; set; }
}
