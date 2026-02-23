using Web.Com.DTOs.Shared;

namespace Web.Com.Services.Interfaces.Shared;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> SignupAsync(SignupRequestDto request);
}
