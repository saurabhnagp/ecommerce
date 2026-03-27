using AmCart.ProductService.Application.DTOs;
using AmCart.ProductService.Domain.Entities;

namespace AmCart.ProductService.Application.Common;

public static class ProductReviewMapping
{
    public static ProductReviewDto ToDto(this ProductReview r, bool? myVoteIsUp = null) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        UserId = r.UserId,
        Rating = r.Rating,
        Title = r.Title,
        Comment = r.Comment,
        IsVerifiedPurchase = r.IsVerifiedPurchase,
        IsApproved = r.IsApproved,
        HelpfulCount = r.HelpfulCount,
        NotHelpfulCount = r.NotHelpfulCount,
        ReviewerDisplayName = r.ReviewerDisplayName,
        ReviewerPhotoUrl = r.ReviewerPhotoUrl,
        MyVote = myVoteIsUp switch { true => "like", false => "dislike", _ => null },
        CreatedAt = r.CreatedAt
    };
}
