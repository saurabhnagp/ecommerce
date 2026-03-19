using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class CreateAddressRequest
{
    [MaxLength(20)]
    public string AddressType { get; set; } = "shipping";

    [MaxLength(50)]
    public string? Label { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required, Phone, MaxLength(20)]
    public string Phone { get; set; } = null!;

    [EmailAddress, MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [Required, MaxLength(255)]
    public string AddressLine1 { get; set; } = null!;

    [MaxLength(255)]
    public string? AddressLine2 { get; set; }

    [MaxLength(255)]
    public string? Landmark { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = null!;

    [Required, MaxLength(100)]
    public string State { get; set; } = null!;

    [Required, MaxLength(20)]
    public string PostalCode { get; set; } = null!;

    [MaxLength(100)]
    public string Country { get; set; } = "India";

    [MaxLength(2)]
    public string CountryCode { get; set; } = "IN";

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }

    [MaxLength(15)]
    public string? Gstin { get; set; }
}
