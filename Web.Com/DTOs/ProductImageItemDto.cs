namespace Web.Com.DTOs;

public class ProductImageItemDto
{
  public Guid Id { get; set; }
  public string Url { get; set; } = string.Empty;
  public bool IsMain { get; set; }
}
