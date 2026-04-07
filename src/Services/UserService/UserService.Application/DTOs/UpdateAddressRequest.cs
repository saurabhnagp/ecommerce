using System.ComponentModel.DataAnnotations;

namespace AmCart.UserService.Application.DTOs;

public class UpdateAddressRequest
{
    [MaxLength(20)]
    public string? AddressType { get; set; }

    [MaxLength(50)]
    public string? Label { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress, MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(255)]
    public string? AddressLine1 { get; set; }

    [MaxLength(255)]
    public string? AddressLine2 { get; set; }

    [MaxLength(255)]
    public string? Landmark { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool? IsDefaultShipping { get; set; }
    public bool? IsDefaultBilling { get; set; }

    [MaxLength(15)]
    public string? Gstin { get; set; }
}
