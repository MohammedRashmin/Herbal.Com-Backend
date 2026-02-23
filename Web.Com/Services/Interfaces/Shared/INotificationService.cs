using Web.Com.Entities;

namespace Web.Com.Services.Interfaces.Shared;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> DeleteNotificationAsync(int notificationId);
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
