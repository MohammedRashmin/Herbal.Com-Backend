using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class BannerController : ControllerBase
{
    private readonly IBannerService _bannerService;

    public BannerController(IBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BannerResponseDto>>> GetBanners()
    {
        var banners = await _bannerService.GetAllBannersAsync();
        return Ok(banners);
    }

    [HttpPost]
    public async Task<ActionResult<BannerResponseDto>> CreateBanner([FromBody] CreateBannerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var banner = await _bannerService.CreateBannerAsync(dto);
        return Ok(new { id = banner.Id, message = "Banner created successfully" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBanner(Guid id, [FromBody] UpdateBannerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _bannerService.UpdateBannerAsync(id, dto);
        if (!result) return NotFound(new { message = "Banner not found" });
        return Ok(new { message = "Banner updated successfully" });
    }

    [HttpPatch("{id}/toggle-active")]
    public async Task<ActionResult> ToggleActive(Guid id, [FromBody] bool isActive)
    {
        var result = await _bannerService.ToggleActiveAsync(id, isActive);
        if (!result) return NotFound(new { message = "Banner not found" });
        return Ok(new { message = "Banner status updated" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBanner(Guid id)
    {
        var result = await _bannerService.DeleteBannerAsync(id);
        if (!result) return NotFound(new { message = "Banner not found" });
        return Ok(new { message = "Banner deleted successfully" });
    }
}
