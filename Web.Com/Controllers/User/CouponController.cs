using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Validation is public - anyone can check a coupon code
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // GET /api/coupon/validate/{code}?cartTotal=99.99
    [HttpGet("validate/{code}")]
    public async Task<ActionResult<ValidateCouponResponseDto>> ValidateCoupon(
        string code,
        [FromQuery] decimal cartTotal = 0)
    {
        var result = await _couponService.ValidateCouponAsync(code, cartTotal);
        return Ok(result);
    }
}
