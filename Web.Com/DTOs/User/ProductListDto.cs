namespace Web.Com.DTOs.User;

public class ProductListDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string ShortDescription { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public decimal? DiscountPrice { get; set; }
  public decimal AverageRating { get; set; }
  public int Stock { get; set; }
  public string? ImageUrl { get; set; }
  public Guid CategoryId { get; set; }
  public string CategoryName { get; set; } = string.Empty;
  public bool IsFeatured { get; set; }
  public bool IsMemberOnly { get; set; }
  public string? Sku { get; set; }
  public DateTime? ExpiryDate { get; set; }
  public string? Weight { get; set; }
}
