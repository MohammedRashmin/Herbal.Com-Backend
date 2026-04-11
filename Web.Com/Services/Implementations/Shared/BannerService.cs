using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Shared;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Services.Implementations.Shared;

public class BannerService : IBannerService
{
    private readonly IBannerRepository _bannerRepository;

    public BannerService(IBannerRepository bannerRepository)
    {
        _bannerRepository = bannerRepository;
    }

    public async Task<IEnumerable<BannerResponseDto>> GetAllBannersAsync()
    {
        var banners = await _bannerRepository.GetAllAsync();
        return banners.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<BannerDto>> GetActiveBannersAsync()
    {
        var banners = await _bannerRepository.GetActiveAsync();
        return banners.Select(b => new BannerDto
        {
            Id = b.Id,
            Title = b.Title,
            Subtitle = b.Subtitle,
            ImageUrl = b.ImageUrl,
            LinkType = b.ProductId != null ? "Product" : b.CategoryId != null ? "Category" : "External",
            LinkValue = b.ProductId?.ToString() ?? b.CategoryId?.ToString() ?? b.ExternalUrl,
            IsActive = b.IsActive,
            Order = b.DisplayOrder
        });
    }

    public async Task<BannerResponseDto> CreateBannerAsync(CreateBannerDto dto)
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
            IsActive = dto.IsActive,
            IsMemberOnly = dto.IsMemberOnly,
            EndDate = dto.EndDate,
            DisplayOrder = dto.DisplayOrder
        };

        await _bannerRepository.AddAsync(banner);
        return MapToResponseDto(banner);
    }

    public async Task<bool> UpdateBannerAsync(Guid id, UpdateBannerDto dto)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner == null) return false;

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

        await _bannerRepository.UpdateAsync(banner);
        return true;
    }

    public async Task<bool> ToggleActiveAsync(Guid id, bool isActive)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner == null) return false;
        banner.IsActive = isActive;
        await _bannerRepository.UpdateAsync(banner);
        return true;
    }

    public async Task<bool> DeleteBannerAsync(Guid id)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner == null) return false;

        await _bannerRepository.DeleteAsync(banner);
        return true;
    }

    private static BannerResponseDto MapToResponseDto(Banner b)
    {
        return new BannerResponseDto
        {
            Id = b.Id,
            Title = b.Title,
            Subtitle = b.Subtitle,
            ImageUrl = b.ImageUrl,
            Tag = b.Tag,
            ProductId = b.ProductId,
            CategoryId = b.CategoryId,
            ExternalUrl = b.ExternalUrl,
            IsMemberOnly = b.IsMemberOnly,
            EndDate = b.EndDate,
            DisplayOrder = b.DisplayOrder,
            IsActive = b.IsActive
        };
    }
}
