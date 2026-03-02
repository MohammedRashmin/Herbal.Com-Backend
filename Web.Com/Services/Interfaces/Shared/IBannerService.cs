using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.Shared;

public interface IBannerService
{
    // Admin
    Task<IEnumerable<BannerResponseDto>> GetAllBannersAsync();
    Task<BannerResponseDto> CreateBannerAsync(CreateBannerDto dto);
    Task<bool> UpdateBannerAsync(Guid id, UpdateBannerDto dto);
    Task<bool> DeleteBannerAsync(Guid id);

    // User
    Task<IEnumerable<BannerDto>> GetActiveBannersAsync();
}

public class BannerResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? ExternalUrl { get; set; }
    public bool IsMemberOnly { get; set; }
    public DateTime? EndDate { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
