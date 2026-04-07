using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmCart.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/users/me/addresses")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var addresses = await _addressService.GetUserAddressesAsync(userId.Value, ct);
        return Ok(new { success = true, data = addresses });
    }

    [HttpGet("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid addressId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _addressService.GetByIdAsync(userId.Value, addressId, ct);
        if (address == null)
            return NotFound(new { success = false, error = new { code = "ADDRESS_NOT_FOUND", message = "Address not found." } });

        return Ok(new { success = true, data = address });
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAddressRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _addressService.CreateAsync(userId.Value, request, ct);
        return Created($"/api/v1/users/me/addresses/{address.Id}", new { success = true, data = address });
    }

    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid addressId, [FromBody] UpdateAddressRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _addressService.UpdateAsync(userId.Value, addressId, request, ct);
        if (address == null)
            return NotFound(new { success = false, error = new { code = "ADDRESS_NOT_FOUND", message = "Address not found." } });

        return Ok(new { success = true, data = address });
    }

    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid addressId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var deleted = await _addressService.DeleteAsync(userId.Value, addressId, ct);
        if (!deleted)
            return NotFound(new { success = false, error = new { code = "ADDRESS_NOT_FOUND", message = "Address not found." } });

        return Ok(new { success = true, message = "Address deleted successfully." });
    }
}
