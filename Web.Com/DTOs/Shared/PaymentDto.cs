namespace Web.Com.DTOs.Shared;

public class CreatePayPalOrderDto
{
    public int OrderId { get; set; }
}

public class PayPalOrderResponseDto
{
    public string PayPalOrderId { get; set; } = string.Empty;
    public string ApprovalUrl { get; set; } = string.Empty;
}

public class ExecutePayPalOrderDto
{
    public string PayPalOrderId { get; set; } = string.Empty;
}

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
}
