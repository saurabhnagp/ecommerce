using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class ContactMessageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Comment { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateContactMessageRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(300)]
    public string Subject { get; set; } = null!;

    [Required]
    public string Comment { get; set; } = null!;
}
