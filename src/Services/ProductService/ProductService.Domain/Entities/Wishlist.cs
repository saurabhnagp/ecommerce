namespace AmCart.ProductService.Domain.Entities;

/// <summary>One wishlist per user (user id comes from UserService JWT).</summary>
public class Wishlist
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
