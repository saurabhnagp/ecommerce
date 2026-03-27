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
    /// <summary>Like / helpful count (from <see cref="ProductReviewVote"/>).</summary>
    public int HelpfulCount { get; set; }
    /// <summary>Dislike count.</summary>
    public int NotHelpfulCount { get; set; }

    /// <summary>Snapshot for storefront display (JWT name at submit time).</summary>
    public string? ReviewerDisplayName { get; set; }
    public string? ReviewerPhotoUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public ICollection<ProductReviewVote> Votes { get; set; } = new List<ProductReviewVote>();
}
