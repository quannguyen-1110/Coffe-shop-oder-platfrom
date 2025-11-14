using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShopAPI.Services;
using CoffeeShopAPI.Repository;
using System.Security.Claims;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/shipper/auth")]
    public class ShipperAuthController : ControllerBase
    {
        private readonly ShipperAuthService _shipperAuthService;
        private readonly UserRepository _userRepository;

        public ShipperAuthController(ShipperAuthService shipperAuthService, UserRepository userRepository)
        {
            _shipperAuthService = shipperAuthService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Đăng nhập cho Shipper (local authentication, không dùng Cognito)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ShipperLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest(new { error = "Username is required" });

                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { error = "Password is required" });

                var result = await _shipperAuthService.LoginAsync(request.Username, request.Password);

                return Ok(new
                {
                    message = "Login successful",
                    token = result.Token,
                    user = new
                    {
                        userId = result.UserId,
                        username = result.Username,
                        fullName = result.FullName,
                        email = result.Email,
                        role = result.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin profile của Shipper đang đăng nhập
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Shipper")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                return Ok(new
                {
                    userId = user.UserId,
                    username = user.Username,
                    fullName = user.FullName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    address = user.Address,
                    vehicleType = user.VehicleType,
                    licensePlate = user.LicensePlate,
                    isActive = user.IsActive,
                    registrationStatus = user.RegistrationStatus,
                    createdAt = user.CreatedAt,
                    approvedAt = user.ApprovedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Đổi mật khẩu cho Shipper
        /// </summary>
        [HttpPost("change-password")]
        [Authorize(Roles = "Shipper")]
        public async Task<IActionResult> ChangePassword([FromBody] ShipperChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                if (string.IsNullOrWhiteSpace(request.OldPassword))
                    return BadRequest(new { error = "Old password is required" });

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest(new { error = "New password is required" });

                if (request.NewPassword.Length < 6)
                    return BadRequest(new { error = "New password must be at least 6 characters" });

                await _shipperAuthService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public class ShipperLoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class ShipperChangePasswordRequest
        {
            public string OldPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
    }
}
