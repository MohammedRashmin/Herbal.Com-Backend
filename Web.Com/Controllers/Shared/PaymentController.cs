using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Web.Com.Data;
using Web.Com.DTOs.Shared;
using Web.Com.Settings;

namespace Web.Com.Controllers.Shared;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly StripeSettings _stripe;

    public PaymentController(AppDbContext context, IOptions<StripeSettings> stripeOptions)
    {
        _context = context;
        _stripe = stripeOptions.Value;
    }

    /// <summary>
    /// Creates a Stripe PaymentIntent for the given order.
    /// Returns clientSecret to the frontend to confirm payment via Stripe.js.
    /// </summary>
    [HttpPost("stripe/create-intent")]
    public async Task<ActionResult<PaymentIntentResponseDto>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.PaymentStatus == "Paid")
            return BadRequest(new { message = "Order is already paid" });

        // Amount in cents (Stripe requires smallest currency unit)
        var amountInCents = (long)(order.TotalAmount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "cad",
            ReceiptEmail = order.User?.Email,
            Description = $"Order #{order.Id}",
            Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() },
                { "userId", userId },
                { "customerName", $"{order.User?.FirstName} {order.User?.LastName}".Trim() }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        // Save PaymentIntentId to order
        order.StripePaymentIntentId = intent.Id;
        order.PaymentMethod = "Card";
        await _context.SaveChangesAsync();

        return Ok(new PaymentIntentResponseDto
        {
            ClientSecret = intent.ClientSecret,
            PaymentIntentId = intent.Id,
            Amount = order.TotalAmount
        });
    }

    /// <summary>
    /// For Cash on Delivery orders — confirms order without payment
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
