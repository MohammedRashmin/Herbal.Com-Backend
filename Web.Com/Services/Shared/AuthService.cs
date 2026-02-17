using Web.Com.DTOs.Shared;
using Web.Com.Entities.Identity;
using Web.Com.Helpers;
using Web.Com.Repositories.Interfaces.Shared;

namespace Web.Com.Services.Shared;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        JwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isValidPassword = await _userRepository.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = await _userRepository.GetUserRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        // Generate and save refresh token
        var refreshToken = Guid.NewGuid().ToString();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "User",
            Roles = roles.ToList()
        };
    }

    public async Task<LoginResponseDto> SignupAsync(SignupRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("All fields are required.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Password and confirm password do not match.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, request.Password);

        var roles = await _userRepository.GetUserRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        // Generate and save refresh token
        var refreshToken = Guid.NewGuid().ToString();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "User",
            Roles = roles.ToList()
        };
    }
}
