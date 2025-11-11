using CoffeeShopAPI.Services;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CoffeeShopAPI.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
private readonly AuthService _authService;
private readonly UserRepository _userRepository;

    public AuthController(AuthService authService, UserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

  /// <summary>
    /// Đăng nhập - Tự động phân biệt Cognito (Customer/Admin) vs Local (Shipper)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
     // Kiểm tra xem có phải Shipper không
        var user = await _userRepository.GetUserByUsernameAsync(req.Username);

        if (user != null && user.Role == "Shipper")
        {
      // ⭐ LOCAL AUTH cho Shipper
    if (string.IsNullOrEmpty(user.PasswordHash))
 return BadRequest(new { error = "Account not activated yet" });

      if (!user.IsActive)
     return Unauthorized(new { error = "Account is locked" });

         if (user.RegistrationStatus != "Approved")
          return Unauthorized(new { error = "Account pending approval" });

            // Verify password
            if (!_authService.VerifyPassword(req.Password, user.PasswordHash))
     return Unauthorized(new { error = "Invalid credentials" });

  // Generate JWT token
            var token = _authService.GenerateJwtToken(user.UserId, user.Username, user.Role);

            return Ok(new
    {
             message = "Login successful (Local Auth)",
  token,
      userId = user.UserId,
           username = user.Username,
      role = user.Role,
           authType = "Local"
       });
      }

    // ⭐ COGNITO AUTH cho Customer & Admin
     try
        {
            var response = await _authService.LoginAsync(req.Username, req.Password);

            if (response.AuthenticationResult == null)
       return Unauthorized(new { error = "Login failed" });

     // Lấy thông tin user từ DynamoDB
            var cognitoUser = await _userRepository.GetUserByUsernameAsync(req.Username);

            return Ok(new
 {
            message = "Login successful (Cognito)",
     accessToken = response.AuthenticationResult.AccessToken,
 idToken = response.AuthenticationResult.IdToken,
      refreshToken = response.AuthenticationResult.RefreshToken,
                expiresIn = response.AuthenticationResult.ExpiresIn,
    userId = cognitoUser?.UserId,
     role = cognitoUser?.Role,
authType = "Cognito"
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
/// Đăng ký tài khoản mới (Cognito + DynamoDB) - Customer/Admin only
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        try
        {
  // 1️ Đăng ký với Cognito
   var result = await _authService.RegisterAsync(req.Username, req.Password, req.Role);

     // 2️ Tạo bản ghi trong DynamoDB
     var newUser = new User
   {
     UserId = result.UserSub, // Cognito ID (sub)
        Username = req.Username,
   Role = req.Role,
        IsActive = false, // Chờ confirm email
                RewardPoints = 0,
       VoucherCount = 0,
    CreatedAt = DateTime.UtcNow
 };

            await _userRepository.AddUserAsync(newUser);

     return Ok(new
            {
message = "Registration successful. Please check your email for confirmation code.",
    userId = result.UserSub,
           userConfirmed = result.UserConfirmed
         });
     }
   catch (Exception ex)
        {
    return BadRequest(new { error = ex.Message });
  }
    }

    /// <summary>
    /// Đăng xuất khỏi Cognito (GlobalSignOut)
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromHeader(Name = "Authorization")] string token)
    {
    try
        {
      if (string.IsNullOrEmpty(token))
          return BadRequest("Access token is required in Authorization header.");

   var accessToken = token.Replace("Bearer ", "").Trim();
            await _authService.GlobalSignOutAsync(accessToken);

            return Ok(new { message = "User logged out successfully!" });
        }
        catch (Exception ex)
  {
        return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmSignUp([FromBody] ConfirmRequest req)
 {
        try
   {
          var result = await _authService.ConfirmSignUpAsync(req.Username, req.ConfirmationCode);
    
   // Kích hoạt user trong DynamoDB
 var user = await _userRepository.GetUserByUsernameAsync(req.Username);
    if (user != null)
        {
         user.IsActive = true;
          await _userRepository.UpdateUserAsync(user);
     }
      
            return Ok(new { message = "User confirmed successfully!" });
        }
     catch (Exception ex)
        {
      return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("resend")]
public async Task<IActionResult> ResendConfirmationCode([FromBody] ResendRequest req)
    {
      try
        {
            var result = await _authService.ResendConfirmationCodeAsync(req.Username);
       return Ok(new { message = "Confirmation code resent successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
      }
    }

    /// <summary>
    /// Shipper đổi mật khẩu (sau khi login)
    /// </summary>
    [Authorize(Roles = "Shipper")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
     try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
     ?? User.FindFirst("sub")?.Value;
   
            if (string.IsNullOrEmpty(userId))
        return Unauthorized(new { error = "Invalid token" });

        var user = await _userRepository.GetUserByIdAsync(userId);
      if (user == null || user.Role != "Shipper")
  return NotFound(new { error = "User not found" });

  // Verify old password
   if (string.IsNullOrEmpty(user.PasswordHash) || 
           !_authService.VerifyPassword(req.OldPassword, user.PasswordHash))
    return BadRequest(new { error = "Invalid old password" });

            // Update password
      user.PasswordHash = _authService.HashPassword(req.NewPassword);
   await _userRepository.UpdateUserAsync(user);

 return Ok(new { message = "Password changed successfully" });
  }
        catch (Exception ex)
        {
      return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
     return Ok(new
        {
    Username = User.Identity?.Name,
Role = User.FindFirst("custom:role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value,
            UserId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        });
    }

    // ===== REQUEST MODELS =====
    
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
    }

    public class ConfirmRequest
    {
     public string Username { get; set; } = string.Empty;
        public string ConfirmationCode { get; set; } = string.Empty;
    }

    public class ResendRequest
    {
  public string Username { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
{
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
    }
}
