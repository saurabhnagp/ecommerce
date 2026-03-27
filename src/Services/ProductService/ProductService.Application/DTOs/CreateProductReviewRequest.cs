using System.ComponentModel.DataAnnotations;

namespace AmCart.ProductService.Application.DTOs;

public class CreateProductReviewRequest
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(300)]
    public string? Title { get; set; }

    [MaxLength(5000)]
    public string? Comment { get; set; }

    public bool IsVerifiedPurchase { get; set; }

    [Url, MaxLength(500)]
    public string? ReviewerPhotoUrl { get; set; }
}
