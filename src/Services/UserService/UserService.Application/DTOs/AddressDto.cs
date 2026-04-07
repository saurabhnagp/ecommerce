namespace AmCart.UserService.Application.DTOs;

public class AddressDto
{
    public Guid Id { get; set; }
    public string AddressType { get; set; } = null!;
    public string? Label { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string? Landmark { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
}
