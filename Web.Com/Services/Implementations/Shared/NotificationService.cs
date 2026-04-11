using Microsoft.AspNetCore.SignalR;
using Web.Com.Entities;
using Web.Com.Hubs;
using Web.Com.Repositories.Interfaces.Shared;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Services.Implementations.Shared;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository notificationRepository,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string userId, string title, string message)
    {
        // Save to DB
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(notification);

        // Push real-time via SignalR
        var dto = new NotificationDto
        {
            Id = notification.Id,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = notification.CreatedAt
        };
        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", dto);
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

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        var n = await _notificationRepository.GetByIdAsync(notificationId);
        if (n == null) return false;
        n.IsRead = true;
        await _notificationRepository.UpdateAsync(n);
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId)
    {
        var n = await _notificationRepository.GetByIdAsync(notificationId);
        if (n == null) return false;
        await _notificationRepository.DeleteAsync(n);
        return true;
    }
}
