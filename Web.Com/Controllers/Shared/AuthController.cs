using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Shared;
using Web.Com.Entities.Identity;
using Web.Com.Helpers;
using Web.Com.Services.Shared;

namespace Web.Com.Controllers.Shared;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService, 
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        JwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _authService = authService;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] SignupRequestDto request)
    {
        try
        {
            var response = await _authService.SignupAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during registration." });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during login." });
        }
    }

    /// <summary>
    /// Logout - invalidates the refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);
        }

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Refresh the JWT token using a valid refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Unauthorized(new { message = "Invalid user" });

        if (user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiry == null || 
            user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        // Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles.ToList());
        var newRefreshToken = Guid.NewGuid().ToString();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Ok(new LoginResponseDto
        {
            Token = token,
            RefreshToken = newRefreshToken,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "User"
        });
    }

    /// <summary>
    /// Signup endpoint (alias for register)
    /// </summary>
    [HttpPost("signup")]
    public async Task<ActionResult<LoginResponseDto>> Signup([FromBody] SignupRequestDto request)
    {
        return await Register(request);
    }

    /// <summary>
    /// Register a new admin account (requires secret key)
    /// </summary>
    [HttpPost("register-admin")]
    public async Task<ActionResult<LoginResponseDto>> RegisterAdmin([FromBody] AdminSignupRequestDto request)
    {
        try
        {
            // Validate admin secret key
            var adminSecretKey = _configuration["AdminSettings:SecretKey"];
            if (string.IsNullOrEmpty(adminSecretKey) || request.AdminSecretKey != adminSecretKey)
            {
                return Unauthorized(new { message = "Invalid admin secret key" });
            }

            // Validate passwords match
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "User with this email already exists" });
            }

            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create admin user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            // Assign Admin role
            await _userManager.AddToRoleAsync(user, "Admin");

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles.ToList());
            var refreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = "Admin"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during admin registration." });
        }
    }

    /// <summary>
    /// Request password reset OTP
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if user exists - return success anyway for security
                return Ok(new ForgotPasswordResponseDto
                {
                    Message = "If the email exists, an OTP has been sent.",
                    ExpiresInMinutes = 10
                });
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var expiryMinutes = 10;

            // Store OTP in database
            user.PasswordResetOtp = otp;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
            await _userManager.UpdateAsync(user);

            // TODO: Send OTP via Email/SMS service
            // await _emailService.SendOtpAsync(user.Email, otp);

            // For development - return OTP in response (REMOVE IN PRODUCTION!)
            return Ok(new ForgotPasswordResponseDto
            {
                Message = "OTP has been sent to your email.",
                Otp = otp, // Remove this line in production
                ExpiresInMinutes = expiryMinutes
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }

    /// <summary>
    /// Reset password using OTP
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Validate OTP
            if (string.IsNullOrEmpty(user.PasswordResetOtp) || 
                user.PasswordResetOtp != request.Otp)
            {
                return BadRequest(new { message = "Invalid OTP" });
            }

            // Check OTP expiry
            if (user.PasswordResetOtpExpiry == null || 
                user.PasswordResetOtpExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "OTP has expired. Please request a new one." });
            }

            // Validate passwords match
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            // Clear OTP after successful reset
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password has been reset successfully. You can now login with your new password." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while resetting password." });
        }
    }
}
