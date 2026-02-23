using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminOrderDto>>> GetOrders([FromQuery] string? status)
    {
        var orders = await _orderService.GetOrdersAsync(status);
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto);
            if (!result) return NotFound(new { message = "Order not found" });
            return Ok(new { message = "Order status updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
