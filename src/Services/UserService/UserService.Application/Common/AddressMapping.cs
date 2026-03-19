using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.Application.Common;

public static class AddressMapping
{
    public static AddressDto ToDto(this Address address) => new()
    {
        Id = address.Id,
        AddressType = address.AddressType,
        Label = address.Label,
        FirstName = address.FirstName,
        LastName = address.LastName,
        Phone = address.Phone,
        Email = address.Email,
        Company = address.Company,
        AddressLine1 = address.AddressLine1,
        AddressLine2 = address.AddressLine2,
        Landmark = address.Landmark,
        City = address.City,
        State = address.State,
        PostalCode = address.PostalCode,
        Country = address.Country,
        CountryCode = address.CountryCode,
        Latitude = address.Latitude,
        Longitude = address.Longitude,
        IsDefaultShipping = address.IsDefaultShipping,
        IsDefaultBilling = address.IsDefaultBilling
    };
}
