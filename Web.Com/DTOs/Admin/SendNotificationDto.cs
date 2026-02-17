using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.Admin;

public class SendNotificationDto
{
    public string? UserId { get; set; } // If null, send to all users

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
}
