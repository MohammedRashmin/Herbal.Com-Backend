using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.Admin;

public interface IOrderService
{
  // Admin
  Task<IEnumerable<AdminOrderDto>> GetOrdersAsync(string? status);
  Task<AdminOrderDetailDto?> GetOrderByIdAdminAsync(Guid id);
  Task<bool> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto);

  // User
  Task<OrderResponseDto> CreateOrderAsync(string userId, CreateOrderDto dto);
  Task<OrderResponseDto> CreateOrderDirectAsync(string userId, CreateOrderDirectDto dto);
  Task<IEnumerable<OrderDto>> GetMyOrdersAsync(string userId);
  Task<OrderDto?> GetOrderByIdAsync(string userId, Guid id);
}

public class OrderResponseDto
{
  public Guid OrderId { get; set; }
  public string Message { get; set; } = string.Empty;
}
