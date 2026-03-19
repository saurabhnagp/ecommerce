using AmCart.UserService.Application.Common;
using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;

namespace AmCart.UserService.Application.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken ct = default)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId, ct);
        return addresses.Select(a => a.ToDto()).ToList();
    }

    public async Task<AddressDto?> GetByIdAsync(Guid userId, Guid addressId, CancellationToken ct = default)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, ct);
        if (address == null || address.UserId != userId) return null;
        return address.ToDto();
    }

    public async Task<AddressDto> CreateAsync(Guid userId, CreateAddressRequest request, CancellationToken ct = default)
    {
        if (request.IsDefaultShipping)
            await ClearDefaultShippingAsync(userId, ct);
        if (request.IsDefaultBilling)
            await ClearDefaultBillingAsync(userId, ct);

        var address = new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AddressType = request.AddressType,
            Label = request.Label,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email,
            Company = request.Company,
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2,
            Landmark = request.Landmark,
            City = request.City.Trim(),
            State = request.State.Trim(),
            PostalCode = request.PostalCode.Trim(),
            Country = request.Country,
            CountryCode = request.CountryCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefaultShipping = request.IsDefaultShipping,
            IsDefaultBilling = request.IsDefaultBilling,
            Gstin = request.Gstin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _addressRepository.AddAsync(address, ct);
        return address.ToDto();
    }

    public async Task<AddressDto?> UpdateAsync(Guid userId, Guid addressId, UpdateAddressRequest request, CancellationToken ct = default)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, ct);
        if (address == null || address.UserId != userId) return null;

        if (request.IsDefaultShipping == true && !address.IsDefaultShipping)
            await ClearDefaultShippingAsync(userId, ct);
        if (request.IsDefaultBilling == true && !address.IsDefaultBilling)
            await ClearDefaultBillingAsync(userId, ct);

        if (request.AddressType != null) address.AddressType = request.AddressType;
        if (request.Label != null) address.Label = request.Label;
        if (request.FirstName != null) address.FirstName = request.FirstName.Trim();
        if (request.LastName != null) address.LastName = request.LastName.Trim();
        if (request.Phone != null) address.Phone = request.Phone.Trim();
        if (request.Email != null) address.Email = request.Email;
        if (request.Company != null) address.Company = request.Company;
        if (request.AddressLine1 != null) address.AddressLine1 = request.AddressLine1.Trim();
        if (request.AddressLine2 != null) address.AddressLine2 = request.AddressLine2;
        if (request.Landmark != null) address.Landmark = request.Landmark;
        if (request.City != null) address.City = request.City.Trim();
        if (request.State != null) address.State = request.State.Trim();
        if (request.PostalCode != null) address.PostalCode = request.PostalCode.Trim();
        if (request.Country != null) address.Country = request.Country;
        if (request.CountryCode != null) address.CountryCode = request.CountryCode;
        if (request.Latitude.HasValue) address.Latitude = request.Latitude;
        if (request.Longitude.HasValue) address.Longitude = request.Longitude;
        if (request.IsDefaultShipping.HasValue) address.IsDefaultShipping = request.IsDefaultShipping.Value;
        if (request.IsDefaultBilling.HasValue) address.IsDefaultBilling = request.IsDefaultBilling.Value;
        if (request.Gstin != null) address.Gstin = request.Gstin;

        address.UpdatedAt = DateTime.UtcNow;
        await _addressRepository.UpdateAsync(address, ct);
        return address.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid addressId, CancellationToken ct = default)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, ct);
        if (address == null || address.UserId != userId) return false;
        await _addressRepository.DeleteAsync(address, ct);
        return true;
    }

    private async Task ClearDefaultShippingAsync(Guid userId, CancellationToken ct)
    {
        var all = await _addressRepository.GetByUserIdAsync(userId, ct);
        foreach (var a in all.Where(a => a.IsDefaultShipping))
        {
            a.IsDefaultShipping = false;
            a.UpdatedAt = DateTime.UtcNow;
            await _addressRepository.UpdateAsync(a, ct);
        }
    }

    private async Task ClearDefaultBillingAsync(Guid userId, CancellationToken ct)
    {
        var all = await _addressRepository.GetByUserIdAsync(userId, ct);
        foreach (var a in all.Where(a => a.IsDefaultBilling))
        {
            a.IsDefaultBilling = false;
            a.UpdatedAt = DateTime.UtcNow;
            await _addressRepository.UpdateAsync(a, ct);
        }
    }
}
