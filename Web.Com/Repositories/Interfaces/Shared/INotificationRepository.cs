using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Shared;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(Notification notification);
}
