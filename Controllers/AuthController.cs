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
    /// Đăng ký tài khoản mới (Cognito + DynamoDB)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string password, string role = "User")
    {
        try
        {
            // 1️ Đăng ký với Cognito
            var result = await _authService.RegisterAsync(username, password, role);

            // 2️ Tạo bản ghi trong DynamoDB
            var newUser = new User
            {
                UserId = result.UserSub, // Cognito ID (sub)
                Username = username,
                Role = role,
                RewardPoints = 0,
                VoucherCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(newUser);

            return Ok(new
            {
                message = " User registered successfully!",
                user = newUser
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Đăng nhập và lấy JWT token từ Cognito
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        try
        {
            var response = await _authService.LoginAsync(username, password);
            return Ok(new
            {
                access_token = response.AuthenticationResult.AccessToken,
                id_token = response.AuthenticationResult.IdToken,
                refresh_token = response.AuthenticationResult.RefreshToken
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

            return Ok(new { message = " User logged out successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmSignUp(string username, string confirmationCode)
    {
        try
        {
            var result = await _authService.ConfirmSignUpAsync(username, confirmationCode);
            return Ok(new { message = "User confirmed successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("resend")]
    public async Task<IActionResult> ResendConfirmationCode(string username)
    {
        try
        {
            var result = await _authService.ResendConfirmationCodeAsync(username);
            return Ok(new { message = "Confirmation code resent successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Admin tạo tài khoản Shipper
    /// </summary>
    [Authorize]
    [HttpPost("admin/create-shipper")]
    public async Task<IActionResult> CreateShipper([FromBody] CreateShipperRequest request)
    {
        try
        {
            // Lấy tên admin từ token Cognito
            string? adminUsername = User.FindFirstValue(ClaimTypes.Email)
                                    ?? User.FindFirstValue("email")
                                    ?? User.FindFirstValue("cognito:username");

            if (string.IsNullOrEmpty(adminUsername))
                return Forbid("Không xác định được admin từ token.");

            // Gọi AuthService để tạo Shipper
            var result = await _authService.CreateShipperAsync(adminUsername, request.Username, request.Password, _userRepository);

            // Tạo bản ghi Shipper trong DynamoDB
            var newUser = new User
            {
                UserId = result.UserSub,
                Username = request.Username,
                Role = "Shipper",
                RewardPoints = 0,
                VoucherCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(newUser);

            return Ok(new
            {
                message = " Shipper account created successfully!",
                shipper = newUser
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class CreateShipperRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                Username = User.Identity?.Name,
                Role = User.FindFirst("custom:role")?.Value
            });
        }

    }
}
