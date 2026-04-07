using System.ComponentModel.DataAnnotations;

namespace AmCart.ProductService.Application.DTOs;

/// <summary>Toggle like/dislike on a review (authenticated). Same action again removes the vote.</summary>
public class ReviewVoteRequest
{
    /// <summary><c>like</c> or <c>dislike</c> (case-insensitive).</summary>
    [Required, MinLength(3), MaxLength(10)]
    public string Action { get; set; } = null!;
}
