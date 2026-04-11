using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Controllers.Shared;

[ApiController]
[Route("api/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IShipStationService _shipStation;
    private readonly INotificationService _notificationService;

    public StripeWebhookController(AppDbContext context, IConfiguration config, IShipStationService shipStation, INotificationService notificationService)
    {
        _context = context;
        _config = config;
        _shipStation = shipStation;
        _notificationService = notificationService;
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
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.StripePaymentIntentId == intent.Id);

        if (order == null || order.PaymentStatus == "Paid") return;

        order.PaymentStatus = "Paid";
        order.Status = OrderStatus.Paid;

        // Reduce stock
        foreach (var item in order.OrderItems)
        {
            item.Product.Stock -= item.Quantity;
        }

        // Clear cart
        var cartItems = _context.CartItems.Where(c => c.UserId == order.UserId);
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        // Push real-time notification
        await _notificationService.SendNotificationAsync(
            order.UserId,
            "Payment Successful",
            $"Your payment for order #{order.Id} has been received. Thank you!");

        // Push to ShipStation
        var shipStationOrderId = await _shipStation.PushOrderAsync(order);
        if (shipStationOrderId != null)
        {
            order.ShipStationOrderId = shipStationOrderId;
            await _context.SaveChangesAsync();
        }
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
