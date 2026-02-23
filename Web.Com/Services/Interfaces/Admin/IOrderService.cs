using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.Admin;

public interface IOrderService
{
    // Admin
    Task<IEnumerable<AdminOrderDto>> GetOrdersAsync(string? status);
    Task<bool> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto);

    // User
    Task<OrderResponseDto> CreateOrderAsync(string userId, CreateOrderDto dto);
    Task<IEnumerable<OrderDto>> GetMyOrdersAsync(string userId);
    Task<OrderDto?> GetOrderByIdAsync(string userId, int id);
}

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
}
