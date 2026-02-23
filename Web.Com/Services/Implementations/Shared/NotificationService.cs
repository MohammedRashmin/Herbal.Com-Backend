using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Shared;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Services.Implementations.Shared;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var n = await _notificationRepository.GetByIdAsync(notificationId);
        if (n == null) return false;

        n.IsRead = true;
        await _notificationRepository.UpdateAsync(n);
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        var n = await _notificationRepository.GetByIdAsync(notificationId);
        if (n == null) return false;

        await _notificationRepository.DeleteAsync(n);
        return true;
    }
}
