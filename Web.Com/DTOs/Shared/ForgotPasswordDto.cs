using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.Shared;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? Otp { get; set; } // Only for development/testing - remove in production
    public int ExpiresInMinutes { get; set; }
}
