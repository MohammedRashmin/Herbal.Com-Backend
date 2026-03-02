namespace Web.Com.DTOs.User;

public class BannerDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string? TargetType { get; set; } // "Product", "Category", "External"
    public Guid? TargetId { get; set; }
    public string? ExternalUrl { get; set; }
    public bool IsMemberOnly { get; set; }
}
