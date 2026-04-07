using System.ComponentModel.DataAnnotations;
using AmCart.ProductService.Application.Validation;

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

    [MaxLength(500), HttpOrRootRelativeUrl]
    public string? ReviewerPhotoUrl { get; set; }
}
