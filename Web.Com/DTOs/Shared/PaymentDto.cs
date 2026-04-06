namespace Web.Com.DTOs.Shared;

// Stripe — Create PaymentIntent
public class CreatePaymentIntentDto
{
    public Guid OrderId { get; set; }
}

public class PaymentIntentResponseDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// Shared result
public class PaymentResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
}
