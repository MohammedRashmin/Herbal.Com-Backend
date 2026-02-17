using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Helpers.Constants;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminOrderDto>>> GetOrders([FromQuery] string? status)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new AdminOrderDto
            {
                Id = o.Id,
                CustomerName = $"{o.User.FirstName} {o.User.LastName}",
                CustomerEmail = o.User.Email ?? string.Empty,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                TrackingNumber = o.TrackingNumber,
                ShippingAddress = o.ShippingAddress,
                PhoneNumber = o.PhoneNumber,
                ItemCount = o.OrderItems.Count
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (!Enum.TryParse<OrderStatus>(dto.Status, out var newStatus))
            return BadRequest(new { message = "Invalid status" });

        var previousStatus = order.Status;
        order.Status = newStatus;

        if (!string.IsNullOrEmpty(dto.TrackingNumber))
            order.TrackingNumber = dto.TrackingNumber;

        // If order is being confirmed, reduce stock (for PayPal orders)
        if (previousStatus == OrderStatus.Pending && newStatus == OrderStatus.Confirmed 
            && order.PaymentStatus == "Paid")
        {
            foreach (var item in order.OrderItems)
            {
                item.Product.Stock -= item.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        // Create notification for user
        var notification = new Notification
        {
            UserId = order.UserId,
            Title = $"Order {newStatus}",
            Message = newStatus switch
            {
                OrderStatus.Confirmed => $"Your order #{order.Id} has been confirmed and is being prepared.",
                OrderStatus.Shipped => $"Your order #{order.Id} has been shipped. Tracking: {order.TrackingNumber}",
                OrderStatus.Delivered => $"Your order #{order.Id} has been delivered. Enjoy!",
                OrderStatus.Cancelled => $"Your order #{order.Id} has been cancelled.",
                _ => $"Your order #{order.Id} status has been updated to {newStatus}."
            }
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Order status updated successfully" });
    }
}
