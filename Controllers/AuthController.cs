using CoffeeShopAPI.Services;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Models;
using Microsoft.AspNetCore.Mvc;

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
                // 1️⃣ Đăng ký với Cognito
                var result = await _authService.RegisterAsync(username, password, role);

                // 2️⃣ Tạo bản ghi trong DynamoDB
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
                    message = "✅ User registered successfully!",
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

                return Ok(new { message = "✅ User logged out successfully!" });
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


    }
}
