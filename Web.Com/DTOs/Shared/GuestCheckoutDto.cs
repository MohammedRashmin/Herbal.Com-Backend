namespace Web.Com.DTOs.Shared;

public class GuestCartItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class GuestCreateOrderDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Card"; // "Card", "COD"
    public List<GuestCartItemDto> Items { get; set; } = new();
}

public class GuestCreatePaymentIntentDto
{
    public Guid OrderId { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class GuestOrderLookupDto
{
    public Guid OrderId { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class GuestOrderSummaryDto
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public List<GuestOrderItemDto> Items { get; set; } = new();
}

public class GuestOrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
}

public class GuestCompleteAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class GuestCompleteAccountResponseDto
{
    public string Message { get; set; } = string.Empty;
    public bool AccountCreated { get; set; }
}
