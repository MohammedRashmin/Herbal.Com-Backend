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

public class AdminOrderDetailDto
{
  public Guid Id { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public string CustomerEmail { get; set; } = string.Empty;
  public string CustomerPhone { get; set; } = string.Empty;
  public DateTime OrderDate { get; set; }
  public decimal Subtotal { get; set; }
  public decimal ShippingFee { get; set; }
  public decimal TotalAmount { get; set; }
  public string Status { get; set; } = string.Empty;
  public string PaymentMethod { get; set; } = string.Empty;
  public string PaymentStatus { get; set; } = string.Empty;
  public string? TrackingNumber { get; set; }
  public string? Carrier { get; set; }
  public string? ShipStationOrderId { get; set; }
  public string ShippingAddress { get; set; } = string.Empty;
  public List<AdminOrderItemDto> Items { get; set; } = new();
}

public class AdminOrderItemDto
{
  public Guid ProductId { get; set; }
  public string ProductName { get; set; } = string.Empty;
  public string? ProductImageUrl { get; set; }
  public int Quantity { get; set; }
  public decimal PriceAtPurchase { get; set; }
}

public class UpdateOrderStatusDto
{
  public string Status { get; set; } = string.Empty; // "Confirmed", "Shipped", "Delivered", "Cancelled"
  public string? TrackingNumber { get; set; }
}
