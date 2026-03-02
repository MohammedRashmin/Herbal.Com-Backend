namespace Web.Com.DTOs.User;

public class ProductDetailDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string ShortDescription { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public decimal? DiscountPrice { get; set; }
  public int Stock { get; set; }
  public string? Weight { get; set; }
  public string? Ingredients { get; set; }
  public decimal AverageRating { get; set; }
  public Guid CategoryId { get; set; }
  public string CategoryName { get; set; } = string.Empty;
  public bool IsFeatured { get; set; }
  public bool IsMemberOnly { get; set; }
  public string? Sku { get; set; }
  public DateTime? ExpiryDate { get; set; }
  public List<string> ImageUrls { get; set; } = new();
  public List<ReviewDto> Reviews { get; set; } = new();
}

public class ReviewDto
{
  public Guid Id { get; set; }
  public string UserName { get; set; } = string.Empty;
  public int Rating { get; set; }
  public string Comment { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
