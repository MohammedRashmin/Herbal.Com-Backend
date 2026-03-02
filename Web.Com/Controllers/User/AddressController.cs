using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.User;
using Web.Com.Services.Interfaces.User;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    // GET /api/address
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressResponseDto>>> GetAddresses()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var addresses = await _addressService.GetAddressesAsync(userId);
        return Ok(addresses);
    }

    // POST /api/address
    [HttpPost]
    public async Task<ActionResult<AddressResponseDto>> CreateAddress([FromBody] CreateAddressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var address = await _addressService.CreateAddressAsync(userId, dto);
        return Ok(address);
    }

    // PUT /api/address/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _addressService.UpdateAddressAsync(userId, id, dto);
        if (!result) return NotFound(new { message = "Address not found" });

        return Ok(new { message = "Address updated successfully" });
    }

    // DELETE /api/address/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAddress(Guid id)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _addressService.DeleteAddressAsync(userId, id);
        if (!result) return NotFound(new { message = "Address not found" });

        return Ok(new { message = "Address deleted successfully" });
    }

    // PATCH /api/address/{id}/set-default
    [HttpPatch("{id}/set-default")]
    public async Task<ActionResult> SetDefault(Guid id)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _addressService.SetDefaultAddressAsync(userId, id);
        if (!result) return NotFound(new { message = "Address not found" });

        return Ok(new { message = "Default address updated" });
    }
}
