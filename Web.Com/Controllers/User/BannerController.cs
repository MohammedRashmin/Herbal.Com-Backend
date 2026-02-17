using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.User;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
public class BannerController : ControllerBase
{
    private readonly AppDbContext _context;

    public BannerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BannerDto>>> GetBanners()
    {
        var banners = await _context.Banners
            .Where(b => b.IsActive && (b.EndDate == null || b.EndDate > DateTime.UtcNow))
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                Subtitle = b.Subtitle,
                ImageUrl = b.ImageUrl,
                Tag = b.Tag,
                TargetType = b.ProductId != null ? "Product" : b.CategoryId != null ? "Category" : b.ExternalUrl != null ? "External" : null,
                TargetId = b.ProductId ?? b.CategoryId,
                ExternalUrl = b.ExternalUrl,
                IsMemberOnly = b.IsMemberOnly
            })
            .ToListAsync();

        return Ok(banners);
    }
}
