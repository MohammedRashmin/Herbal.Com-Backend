using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Settings;
using Microsoft.Extensions.Options;

namespace Web.Com.Controllers.Shared;

[ApiController]
[Route("api/webhooks")]
[IgnoreAntiforgeryToken]
public class ShipStationWebhookController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ShipStationWebhookController> _logger;
    private readonly ShipStationSettings _settings;

    public ShipStationWebhookController(AppDbContext context, ILogger<ShipStationWebhookController> logger, IOptions<ShipStationSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    [HttpPost("shipstation")]
    public async Task<IActionResult> Handle()
    {
        // ShipStation sends either form-encoded or JSON depending on webhook version
        string resource_url = string.Empty;
        string resource_type = string.Empty;

        var contentType = Request.ContentType ?? "";
        if (contentType.Contains("application/json"))
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation("ShipStation webhook JSON body: {Body}", body);
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            resource_url  = doc.RootElement.TryGetProperty("resource_url",  out var u) ? u.GetString() ?? "" : "";
            resource_type = doc.RootElement.TryGetProperty("resource_type", out var t) ? t.GetString() ?? "" : "";
        }
        else
        {
            resource_url  = Request.Form["resource_url"].ToString();
            resource_type = Request.Form["resource_type"].ToString();
        }

        _logger.LogInformation("ShipStation webhook received: {Type} → {Url}", resource_type, resource_url);

        if (string.IsNullOrEmpty(resource_url) || string.IsNullOrEmpty(resource_type))
            return Ok(); // Return 200 so ShipStation doesn't retry

        switch (resource_type)
        {
            case "SHIP_NOTIFY":
                await HandleShipNotify(resource_url);
                break;
            case "ORDER_DELIVERED":
                await HandleOrderDelivered(resource_url);
                break;
            default:
                _logger.LogInformation("Unhandled ShipStation event: {Type}", resource_type);
                break;
        }

        return Ok();
    }

    private async Task HandleShipNotify(string resourceUrl)
    {
        var data = await FetchResource(resourceUrl);
        if (data == null) return;

        // Response: { "shipments": [ { "orderNumber": "...", "trackingNumber": "...", "carrierCode": "..." } ] }
        if (!data.RootElement.TryGetProperty("shipments", out var shipments)) return;

        foreach (var shipment in shipments.EnumerateArray())
        {
            if (shipment.TryGetProperty("voided", out var voided) && voided.GetBoolean()) continue;

            var orderNumber = shipment.TryGetProperty("orderNumber", out var on) ? on.GetString() : null;
            var trackingNumber = shipment.TryGetProperty("trackingNumber", out var tn) ? tn.GetString() : null;
            var carrierCode = shipment.TryGetProperty("carrierCode", out var cc) ? cc.GetString() : null;

            if (!Guid.TryParse(orderNumber, out var orderId)) continue;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered) continue;

            order.Status = OrderStatus.Shipped;
            order.TrackingNumber = trackingNumber;
            order.Carrier = carrierCode;

            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Order Shipped",
                Message = $"Your order #{order.Id} has been shipped! Tracking: {trackingNumber} ({carrierCode})"
            });

            _logger.LogInformation("Order {OrderId} marked Shipped. Tracking: {Tracking}", orderId, trackingNumber);
        }

        await _context.SaveChangesAsync();
    }

    private async Task HandleOrderDelivered(string resourceUrl)
    {
        var data = await FetchResource(resourceUrl);
        if (data == null) return;

        // Response: { "fulfillments": [ { "orderNumber": "...", "deliveryDate": "..." } ] }
        var arrayProp = data.RootElement.TryGetProperty("fulfillments", out var fulfillments)
            ? fulfillments
            : data.RootElement.TryGetProperty("shipments", out var shipments2) ? shipments2 : default;

        if (arrayProp.ValueKind != JsonValueKind.Array) return;

        foreach (var item in arrayProp.EnumerateArray())
        {
            var orderNumber = item.TryGetProperty("orderNumber", out var on) ? on.GetString() : null;
            if (!Guid.TryParse(orderNumber, out var orderId)) continue;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.Status == OrderStatus.Delivered) continue;

            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;

            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Order Delivered",
                Message = $"Your order #{order.Id} has been delivered. Thank you for shopping with us!"
            });

            _logger.LogInformation("Order {OrderId} marked Delivered at {Time}", orderId, order.DeliveredAt);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<JsonDocument?> FetchResource(string url)
    {
        try
        {
            using var client = new HttpClient();
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.ApiKey}:{_settings.ApiSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ShipStation resource fetch failed: {Status}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("ShipStation resource payload: {Json}", json);
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch ShipStation resource: {Url}", url);
            return null;
        }
    }
}
