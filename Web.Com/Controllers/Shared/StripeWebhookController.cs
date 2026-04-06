using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Web.Com.Data;
using Web.Com.Entities;

namespace Web.Com.Controllers.Shared;

[ApiController]
[Route("api/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public StripeWebhookController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Handle()
    {
        // Read raw body as bytes — preserves exact bytes Stripe signed
        string json;
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        json = Encoding.UTF8.GetString(ms.ToArray());
        var webhookSecret = _config["Stripe:WebhookSecret"];

        Event stripeEvent;
        try
        {
            var sigHeader = Request.Headers["Stripe-Signature"];
            stripeEvent = EventUtility.ConstructEvent(json, sigHeader, webhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException)
        {
            return BadRequest(new { message = "Invalid Stripe signature" });
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentSucceeded(stripeEvent);
                break;

            case "payment_intent.payment_failed":
                await HandlePaymentFailed(stripeEvent);
                break;
        }

        return Ok();
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent == null) return;

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.StripePaymentIntentId == intent.Id);

        if (order == null || order.PaymentStatus == "Paid") return;

        order.PaymentStatus = "Paid";
        order.Status = OrderStatus.Confirmed;

        // Reduce stock
        foreach (var item in order.OrderItems)
        {
            item.Product.Stock -= item.Quantity;
        }

        // Clear cart
        var cartItems = _context.CartItems.Where(c => c.UserId == order.UserId);
        _context.CartItems.RemoveRange(cartItems);

        // Notification
        _context.Notifications.Add(new Notification
        {
            UserId = order.UserId,
            Title = "Payment Successful",
            Message = $"Your payment for order #{order.Id} has been received. Thank you!"
        });

        await _context.SaveChangesAsync();
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent == null) return;

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.StripePaymentIntentId == intent.Id);

        if (order == null) return;

        order.PaymentStatus = "Failed";
        await _context.SaveChangesAsync();
    }
}
