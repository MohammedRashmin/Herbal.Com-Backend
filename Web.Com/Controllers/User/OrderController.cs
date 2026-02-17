using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.User;
using Web.Com.Entities;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return BadRequest(new { message = "Cart is empty" });

        // Check stock availability
        foreach (var item in cartItems)
        {
            if (item.Product.Stock < item.Quantity)
                return BadRequest(new { message = $"Insufficient stock for {item.Product.Name}" });
        }

        // Calculate total
        decimal total = cartItems.Sum(c => 
            (c.Product.DiscountPrice ?? c.Product.Price) * c.Quantity);

        // Create order
        var order = new Order
        {
            UserId = userId,
            TotalAmount = total,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = dto.PaymentMethod == "COD" ? "COD" : "Pending",
            ShippingAddress = dto.ShippingAddress,
            PhoneNumber = dto.PhoneNumber,
            Status = OrderStatus.Pending
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create order items
        foreach (var cartItem in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                PriceAtPurchase = cartItem.Product.DiscountPrice ?? cartItem.Product.Price
            };
            _context.OrderItems.Add(orderItem);

            // Reduce stock (only if COD, otherwise after payment)
            if (dto.PaymentMethod == "COD")
            {
                cartItem.Product.Stock -= cartItem.Quantity;
            }
        }

        // Clear cart (only if COD)
        if (dto.PaymentMethod == "COD")
        {
            _context.CartItems.RemoveRange(cartItems);
        }

        await _context.SaveChangesAsync();

        // Create notification
        var notification = new Notification
        {
            UserId = userId,
            Title = "Order Created",
            Message = $"Your order #{order.Id} has been created successfully."
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return Ok(new { orderId = order.Id, message = "Order created successfully" });
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                TrackingNumber = o.TrackingNumber,
                ShippingAddress = o.ShippingAddress,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.Images.FirstOrDefault() != null ? oi.Product.Images.First().ImageUrl : null,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        var dto = new OrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            TrackingNumber = order.TrackingNumber,
            ShippingAddress = order.ShippingAddress,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                ProductImageUrl = oi.Product.Images.FirstOrDefault() != null ? oi.Product.Images.First().ImageUrl : null,
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.PriceAtPurchase
            }).ToList()
        };

        return Ok(dto);
    }
}
