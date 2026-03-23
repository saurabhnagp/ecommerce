namespace AmCart.UserService.Domain.Entities;

public class Testimonial
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string Comment { get; set; } = null!;
    public int Rating { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
