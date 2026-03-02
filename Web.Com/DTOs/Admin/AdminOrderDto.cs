namespace Web.Com.DTOs.Admin;

public class AdminOrderDto
{
  public Guid Id { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public string CustomerEmail { get; set; } = string.Empty;
  public DateTime OrderDate { get; set; }
  public decimal TotalAmount { get; set; }
  public string Status { get; set; } = string.Empty;
  public string PaymentMethod { get; set; } = string.Empty;
  public string PaymentStatus { get; set; } = string.Empty;
  public string? TrackingNumber { get; set; }
  public string ShippingAddress { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public int ItemCount { get; set; }
}

public class UpdateOrderStatusDto
{
  public string Status { get; set; } = string.Empty; // "Confirmed", "Shipped", "Delivered", "Cancelled"
  public string? TrackingNumber { get; set; }
}
