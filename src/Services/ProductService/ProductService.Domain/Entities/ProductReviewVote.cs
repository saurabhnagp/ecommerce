namespace AmCart.ProductService.Domain.Entities;

public class ProductReviewVote
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; }
    public bool IsUp { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProductReview Review { get; set; } = null!;
}
