using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.Shared;
using Web.Com.Entities;

namespace Web.Com.Controllers.Shared;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public PaymentController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Creates a PayPal order for the given local order
    /// In production, this would call PayPal API to create an order
    /// </summary>
    [HttpPost("paypal/create")]
    public async Task<ActionResult<PayPalOrderResponseDto>> CreatePayPalOrder([FromBody] CreatePayPalOrderDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.PaymentStatus == "Paid")
            return BadRequest(new { message = "Order is already paid" });

        // In production, you would call PayPal API here:
        // var paypalClient = new PayPalHttpClient(environment);
        // var request = new OrdersCreateRequest();
        // var result = await paypalClient.Execute(request);

        // For now, we simulate the PayPal order creation
        var paypalOrderId = $"PAYPAL-{Guid.NewGuid():N}".ToUpper();
        
        order.PayPalOrderId = paypalOrderId;
        await _context.SaveChangesAsync();

        // In production, the approval URL would come from PayPal
        var approvalUrl = $"https://www.sandbox.paypal.com/checkoutnow?token={paypalOrderId}";

        return Ok(new PayPalOrderResponseDto
        {
            PayPalOrderId = paypalOrderId,
            ApprovalUrl = approvalUrl
        });
    }

    /// <summary>
    /// Executes/captures the PayPal payment after user approval
    /// In production, this would call PayPal API to capture the payment
    /// </summary>
    [HttpPost("paypal/execute")]
    public async Task<ActionResult<PaymentResultDto>> ExecutePayPalOrder([FromBody] ExecutePayPalOrderDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.PayPalOrderId == dto.PayPalOrderId && o.UserId == userId);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.PaymentStatus == "Paid")
            return BadRequest(new { message = "Order is already paid" });

        // In production, you would call PayPal API to capture the payment:
        // var request = new OrdersCaptureRequest(dto.PayPalOrderId);
        // var result = await paypalClient.Execute(request);
        // if (result.Result.Status == "COMPLETED") { ... }

        // Simulate successful payment
        order.PaymentStatus = "Paid";
        order.Status = OrderStatus.Confirmed;

        // Reduce stock
        foreach (var item in order.OrderItems)
        {
            item.Product.Stock -= item.Quantity;
        }

        // Clear user's cart
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();
        _context.CartItems.RemoveRange(cartItems);

        // Create notification
        var notification = new Notification
        {
            UserId = userId,
            Title = "Payment Successful",
            Message = $"Your payment for order #{order.Id} has been received. Thank you!"
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return Ok(new PaymentResultDto
        {
            Success = true,
            Message = "Payment successful",
            OrderId = order.Id
        });
    }

    /// <summary>
    /// For Cash on Delivery orders - confirms the order without payment
    /// </summary>
    [HttpPost("cod")]
    public async Task<ActionResult<PaymentResultDto>> ConfirmCODOrder([FromQuery] Guid orderId)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.PaymentMethod != "COD")
            return BadRequest(new { message = "This is not a COD order" });

        return Ok(new PaymentResultDto
        {
            Success = true,
            Message = "COD order confirmed. Pay on delivery.",
            OrderId = order.Id
        });
    }
}
