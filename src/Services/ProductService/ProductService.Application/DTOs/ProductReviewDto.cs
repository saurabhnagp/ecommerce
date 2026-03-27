namespace AmCart.ProductService.Application.DTOs;

public class ProductReviewDto
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
    public int NotHelpfulCount { get; set; }
    public string? ReviewerDisplayName { get; set; }
    public string? ReviewerPhotoUrl { get; set; }
    /// <summary>Current user's vote on this review: <c>like</c>, <c>dislike</c>, or omitted/null.</summary>
    public string? MyVote { get; set; }
    public DateTime CreatedAt { get; set; }
}
