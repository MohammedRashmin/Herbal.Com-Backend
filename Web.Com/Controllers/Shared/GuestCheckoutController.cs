using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Web.Com.Data;
using Web.Com.DTOs.Shared;
using Web.Com.Entities.Identity;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.Shared;

/// <summary>
/// Home Depot-style guest checkout endpoints.
/// All endpoints are anonymous; guest identity is keyed off the email + orderId.
/// </summary>
[ApiController]
[Route("api/guest")]
[AllowAnonymous]
public class GuestCheckoutController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOrderService _orderService;

    public GuestCheckoutController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IOrderService orderService)
    {
        _context = context;
        _userManager = userManager;
        _orderService = orderService;
    }

    /// <summary>
    /// Create a guest order. Server will find-or-create a user record by email
    /// (no password is set) so the existing order pipeline works unchanged.
    /// </summary>
    [HttpPost("checkout/create")]
    public async Task<ActionResult<OrderResponseDto>> CreateGuestOrder(
        [FromBody] GuestCreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(dto.ShippingAddress) ||
            string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest(new { message = "Shipping address and phone are required" });

        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { message = "At least one item is required" });

        var email = dto.Email.Trim().ToLowerInvariant();

        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? "Guest" : dto.FirstName.Trim(),
                    LastName = string.IsNullOrWhiteSpace(dto.LastName) ? "Customer" : dto.LastName.Trim(),
                    EmailConfirmed = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                // Create user without password — guest cannot log in until they set one
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return StatusCode(500, new { message = "Could not create guest profile" });
                }
            }

            var response = await _orderService.CreateGuestOrderAsync(user.Id, dto);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating your order." });
        }
    }

    /// <summary>
    /// Create a Stripe PaymentIntent for a guest order. Verifies the email
    /// matches the order's user before creating the intent. Currency is CAD.
    /// </summary>
    [HttpPost("payment/stripe/create-intent")]
    public async Task<ActionResult<PaymentIntentResponseDto>> CreateGuestPaymentIntent(
        [FromBody] GuestCreatePaymentIntentDto dto)
    {
        if (dto.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Order id and email are required" });

        var email = dto.Email.Trim().ToLowerInvariant();

        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (!string.Equals(order.User?.Email, email, StringComparison.OrdinalIgnoreCase))
            return NotFound(new { message = "Order not found" });

        if (order.PaymentStatus == "Paid")
            return BadRequest(new { message = "Order is already paid" });

        var amountInCents = (long)(order.TotalAmount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "cad",
            ReceiptEmail = order.User?.Email,
            Description = $"Order #{order.Id} (guest)",
            Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() },
                { "userId", order.UserId },
                { "guest", "true" },
                { "customerName", $"{order.User?.FirstName} {order.User?.LastName}".Trim() }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

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
    /// Look up a guest order using order id + email. Returns a redacted summary.
    /// Returns 404 for any mismatch to avoid leaking which orders/emails exist.
    /// </summary>
    [HttpPost("orders/lookup")]
    public async Task<ActionResult<GuestOrderSummaryDto>> LookupGuestOrder(
        [FromBody] GuestOrderLookupDto dto)
    {
        if (dto.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Order id and email are required" });

        var email = dto.Email.Trim().ToLowerInvariant();
        var summary = await _orderService.GetGuestOrderAsync(dto.OrderId, email);

        if (summary == null)
            return NotFound(new { message = "We couldn't find that order. Please check your details." });

        return Ok(summary);
    }

    /// <summary>
    /// Optional post-purchase account completion: lets a guest set a password
    /// for the user record that was created during guest checkout. Only allowed
    /// if the user currently has no password.
    /// </summary>
    [HttpPost("auth/complete-account")]
    public async Task<ActionResult<GuestCompleteAccountResponseDto>> CompleteGuestAccount(
        [FromBody] GuestCompleteAccountDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email and password are required" });

        if (dto.Password != dto.ConfirmPassword)
            return BadRequest(new { message = "Passwords do not match" });

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Generic message to avoid disclosing which emails exist
            return BadRequest(new { message = "Could not complete account creation." });
        }

        if (await _userManager.HasPasswordAsync(user))
        {
            return Conflict(new { message = "An account already exists. Please sign in instead." });
        }

        var addResult = await _userManager.AddPasswordAsync(user, dto.Password);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            return BadRequest(new { message = errors });
        }

        // Make sure the user has the Customer role so they can sign in normally
        if (!await _userManager.IsInRoleAsync(user, "Customer"))
        {
            await _userManager.AddToRoleAsync(user, "Customer");
        }

        return Ok(new GuestCompleteAccountResponseDto
        {
            Message = "Account created. You can now sign in with your email and password.",
            AccountCreated = true,
        });
    }
}
