using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.User;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
public class BannerController : ControllerBase
{
    private readonly IBannerService _bannerService;

    public BannerController(IBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BannerDto>>> GetBanners()
    {
        var banners = await _bannerService.GetActiveBannersAsync();
        return Ok(banners);
    }
}
