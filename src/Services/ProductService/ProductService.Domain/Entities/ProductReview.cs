namespace AmCart.ProductService.Domain.Entities;

public class ProductReview
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }

    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }

    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
    public int HelpfulCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
