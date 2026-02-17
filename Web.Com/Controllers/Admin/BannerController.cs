using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Helpers.Constants;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class BannerController : ControllerBase
{
    private readonly AppDbContext _context;

    public BannerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetBanners()
    {
        var banners = await _context.Banners
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Subtitle,
                b.ImageUrl,
                b.Tag,
                b.ProductId,
                b.CategoryId,
                b.ExternalUrl,
                b.IsMemberOnly,
                b.EndDate,
                b.DisplayOrder,
                b.IsActive
            })
            .ToListAsync();

        return Ok(banners);
    }

    [HttpPost]
    public async Task<ActionResult> CreateBanner([FromBody] CreateBannerDto dto)
    {
        var banner = new Banner
        {
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ImageUrl = dto.ImageUrl,
            Tag = dto.Tag,
            ProductId = dto.ProductId,
            CategoryId = dto.CategoryId,
            ExternalUrl = dto.ExternalUrl,
            IsMemberOnly = dto.IsMemberOnly,
            EndDate = dto.EndDate,
            DisplayOrder = dto.DisplayOrder
        };

        _context.Banners.Add(banner);
        await _context.SaveChangesAsync();

        return Ok(new { id = banner.Id, message = "Banner created successfully" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBanner(int id, [FromBody] UpdateBannerDto dto)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
            return NotFound(new { message = "Banner not found" });

        banner.Title = dto.Title;
        banner.Subtitle = dto.Subtitle;
        banner.ImageUrl = dto.ImageUrl;
        banner.Tag = dto.Tag;
        banner.ProductId = dto.ProductId;
        banner.CategoryId = dto.CategoryId;
        banner.ExternalUrl = dto.ExternalUrl;
        banner.IsMemberOnly = dto.IsMemberOnly;
        banner.EndDate = dto.EndDate;
        banner.DisplayOrder = dto.DisplayOrder;
        banner.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Banner updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBanner(int id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
            return NotFound(new { message = "Banner not found" });

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Banner deleted successfully" });
    }
}
