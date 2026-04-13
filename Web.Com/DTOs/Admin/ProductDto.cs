namespace Web.Com.DTOs.Admin;

public class ProductDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string ShortDescription { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public Guid CategoryId { get; set; }
  public string CategoryName { get; set; } = string.Empty;
  public int Stock { get; set; }
  public decimal Price { get; set; }
  public decimal? DiscountPrice { get; set; }
  public string? Weight { get; set; }
  public string? BatchNumber { get; set; }
  public string? Ingredients { get; set; }
  public bool IsFeatured { get; set; }
  public bool IsMemberOnly { get; set; }
  public string? Sku { get; set; }
  public DateTime? ExpiryDate { get; set; }
  public string? ServingSize { get; set; }
  public int? ServingsPerContainer { get; set; }
  public List<string> Badges { get; set; } = new();
  public List<string> Benefits { get; set; } = new();
  public List<string> ImageUrls { get; set; } = new();
}
