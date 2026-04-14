using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Web.Com.Entities;
using Web.Com.Services.Interfaces.Shared;
using Web.Com.Settings;

namespace Web.Com.Services.Implementations.Shared;

public class ShipStationService : IShipStationService
{
    private readonly HttpClient _http;
    private readonly ILogger<ShipStationService> _logger;
    private readonly ShipStationSettings _settings;

    public ShipStationService(HttpClient http, IOptions<ShipStationSettings> settings, ILogger<ShipStationService> logger)
    {
        _http = http;
        _logger = logger;
        _settings = settings.Value;

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.ApiKey}:{_settings.ApiSecret}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        _http.BaseAddress = new Uri(_settings.BaseUrl);
    }

    public async Task<string?> PushOrderAsync(Order order)
    {
        var payload = BuildOrderPayload(order);
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Retry up to 3 times if ShipStation is down
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var response = await _http.PostAsync("/orders/createorder", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("ShipStation push attempt {Attempt} for order {OrderId}: {Status}", attempt, order.Id, response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("orderId", out var orderIdProp))
                        return orderIdProp.GetRawText();
                    return null;
                }

                _logger.LogWarning("ShipStation returned {Status}: {Body}", response.StatusCode, responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ShipStation push attempt {Attempt} failed for order {OrderId}", attempt, order.Id);
            }

            if (attempt < 3)
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
        }

        return null;
    }

    private static string ToCountryCode(string country)
    {
        if (country.Length == 2) return country.ToUpper();
        return country.ToLower() switch
        {
            "united states" or "usa" or "us"          => "US",
            "india" or "in"                            => "IN",
            "united kingdom" or "uk" or "gb"           => "GB",
            "canada" or "ca"                           => "CA",
            "australia" or "au"                        => "AU",
            "germany" or "de"                          => "DE",
            "france" or "fr"                           => "FR",
            "singapore" or "sg"                        => "SG",
            "uae" or "united arab emirates"            => "AE",
            "malaysia" or "my"                         => "MY",
            _                                          => "CA"
        };
    }

    private object BuildOrderPayload(Order order)
    {
        // Parse flat ShippingAddress: "FirstName LastName, Street, City, State ZIP, Country"
        var parts = order.ShippingAddress.Split(',', StringSplitOptions.TrimEntries);
        var parsedName = parts.Length > 0 ? parts[0].Trim() : "";
        var userFullName = $"{order.User?.FirstName} {order.User?.LastName}".Trim();
        var name = !string.IsNullOrEmpty(parsedName) ? parsedName
                 : !string.IsNullOrEmpty(userFullName) ? userFullName
                 : "Customer";
        var street     = parts.Length > 1 ? parts[1] : "";
        var city       = parts.Length > 2 ? parts[2] : "";
        var stateZip   = parts.Length > 3 ? parts[3].Split(' ', 2, StringSplitOptions.TrimEntries) : Array.Empty<string>();
        var state      = stateZip.Length > 0 ? stateZip[0] : "";
        var postalCode = stateZip.Length > 1 ? stateZip[1] : "";
        var countryRaw = parts.Length > 4 ? parts[4].Trim() : "US";
        var country    = ToCountryCode(countryRaw);

        // Total weight in grams (Product.Weight is string, assumed in kg)
        static double ParseWeight(string? w)
        {
            if (string.IsNullOrWhiteSpace(w)) return 500;
            var clean = w.Trim().ToLowerInvariant();
            // Extract numeric part
            var numStr = new string(clean.Where(c => c == '.' || c == ',' || char.IsDigit(c)).ToArray());
            if (!decimal.TryParse(numStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value) || value <= 0)
                return 500;
            // If unit is kg, convert to grams
            if (clean.Contains("kg")) return (double)value * 1000;
            // Otherwise assume grams
            return (double)value;
        }

        var totalWeightGrams = order.OrderItems
            .Sum(i => ParseWeight(i.Product?.Weight) * i.Quantity);
        if (totalWeightGrams <= 0) totalWeightGrams = 500;

        var items = order.OrderItems.Select(i => new
        {
            lineItemKey = i.Id.ToString(),
            name        = i.Product?.Name ?? "Product",
            sku         = i.ProductId.ToString(),
            quantity    = i.Quantity,
            unitPrice   = i.PriceAtPurchase,
            weight = new { value = ParseWeight(i.Product?.Weight), units = "grams" }
        }).ToList();

        var customerEmail = order.User?.Email ?? "";
        var customerPhone = order.PhoneNumber ?? order.User?.PhoneNumber ?? "";

        return new
        {
            orderNumber    = order.Id.ToString(),
            orderDate      = order.OrderDate.ToString("yyyy-MM-ddTHH:mm:ss.000Z"),
            orderStatus    = "awaiting_shipment",
            customerEmail,
            customerUsername = customerEmail,
            amountPaid     = order.TotalAmount,
            taxAmount      = 0,
            shippingAmount = 0,
            internalNotes  = $"Order placed via Herbal.Com | Payment: {order.PaymentMethod}",
            weight = new { value = totalWeightGrams, units = "grams" },
            billTo = new
            {
                name,
                email = customerEmail,
                phone = customerPhone,
            },
            shipTo = new
            {
                name,
                street1    = street,
                city,
                state,
                postalCode,
                country    = string.IsNullOrEmpty(country) ? "US" : country,
                phone      = customerPhone,
            },
            items
        };
    }
}
