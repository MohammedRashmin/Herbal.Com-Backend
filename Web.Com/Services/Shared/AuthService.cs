using Web.Com.DTOs.Shared;
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
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check if password hash matches
        var isValidPassword = await _userRepository.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user);

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return new LoginResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };
    }

    public async Task<LoginResponseDto> SignupAsync(SignupRequestDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("All fields are required.");
        }

        // Check if passwords match
        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Password and confirm password do not match.");
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        // Create new user
        var user = new Models.Identity.ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, request.Password);

        // Get user roles (default: Customer)
        var roles = await _userRepository.GetUserRolesAsync(user);

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return new LoginResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };
    }
}
