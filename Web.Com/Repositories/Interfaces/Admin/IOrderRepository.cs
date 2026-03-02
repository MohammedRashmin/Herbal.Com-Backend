using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Admin;

public interface IOrderRepository
{
  Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status = null);
  Task<Order?> GetByIdAsync(Guid id);
  Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
  Task<Order?> GetByIdAndUserIdAsync(Guid id, string userId);
  Task<Order> CreateAsync(Order order);
  Task UpdateAsync(Order order);
  Task AddNotificationAsync(Notification notification);
}
