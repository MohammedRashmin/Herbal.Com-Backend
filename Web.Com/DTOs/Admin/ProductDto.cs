namespace Web.Com.DTOs.Admin;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? Weight { get; set; }
    public string? Ingredients { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsMemberOnly { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}
