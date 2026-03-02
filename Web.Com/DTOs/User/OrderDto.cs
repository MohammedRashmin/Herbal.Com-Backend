namespace Web.Com.DTOs.User;

public class CreateOrderDto
{
  public string ShippingAddress { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public string PaymentMethod { get; set; } = string.Empty; // "PayPal", "COD"
}

public class OrderDto
{
  public Guid Id { get; set; }
  public DateTime OrderDate { get; set; }
  public decimal TotalAmount { get; set; }
  public string Status { get; set; } = string.Empty;
  public string PaymentMethod { get; set; } = string.Empty;
  public string PaymentStatus { get; set; } = string.Empty;
  public string? TrackingNumber { get; set; }
  public string ShippingAddress { get; set; } = string.Empty;
  public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
  public Guid ProductId { get; set; }
  public string ProductName { get; set; } = string.Empty;
  public string? ProductImageUrl { get; set; }
  public int Quantity { get; set; }
  public decimal PriceAtPurchase { get; set; }
}
