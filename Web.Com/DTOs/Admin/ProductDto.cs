namespace Web.Com.DTOs.Admin;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}
