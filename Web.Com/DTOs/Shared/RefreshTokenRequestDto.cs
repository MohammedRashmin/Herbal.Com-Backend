using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.Shared;

public class RefreshTokenRequestDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
