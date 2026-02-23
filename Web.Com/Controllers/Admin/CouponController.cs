using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // GET /api/admin/coupon
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CouponResponseDto>>> GetAllCoupons()
    {
        var coupons = await _couponService.GetAllCouponsAsync();
        return Ok(coupons);
    }

    // GET /api/admin/coupon/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CouponResponseDto>> GetCoupon(int id)
    {
        var coupon = await _couponService.GetCouponByIdAsync(id);
        if (coupon == null)
            return NotFound(new { message = "Coupon not found" });

        return Ok(coupon);
    }

    // POST /api/admin/coupon
    [HttpPost]
    public async Task<ActionResult<CouponResponseDto>> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var coupon = await _couponService.CreateCouponAsync(dto);
        return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, coupon);
    }

    // PUT /api/admin/coupon/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _couponService.UpdateCouponAsync(id, dto);
        if (!result)
            return NotFound(new { message = "Coupon not found" });

        return Ok(new { message = "Coupon updated successfully" });
    }

    // DELETE /api/admin/coupon/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCoupon(int id)
    {
        var result = await _couponService.DeleteCouponAsync(id);
        if (!result)
            return NotFound(new { message = "Coupon not found" });

        return Ok(new { message = "Coupon deleted successfully" });
    }
}
