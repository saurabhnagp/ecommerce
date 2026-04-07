namespace AmCart.UserService.Application.DTOs;

public class TestimonialDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string Comment { get; set; } = null!;
    public int Rating { get; set; }
    public int SortOrder { get; set; }
}

public class CreateTestimonialRequest
{
    public string CustomerName { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string Comment { get; set; } = null!;
    public int Rating { get; set; } = 5;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTestimonialRequest
{
    public string? CustomerName { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Comment { get; set; }
    public int? Rating { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}
