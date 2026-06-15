namespace Web.Com.DTOs.User;

public class BannerDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkType { get; set; }   // "Product", "Category", "External"
    public string? LinkValue { get; set; }  // productId, categoryId, or external URL
    public string Tag { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
}
