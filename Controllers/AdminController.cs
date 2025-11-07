using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] //  chỉ admin mới có quyền truy cập
    public class AdminController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;

        public AdminController(UserRepository userRepository, AuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        /// <summary>
        ///  Lấy danh sách tất cả Shipper
        /// </summary>
        [HttpGet("shippers")]
        public async Task<IActionResult> GetShippers()
        {
            var shippers = await _userRepository.GetUsersByRoleAsync("Shipper");
            return Ok(shippers);
        }

        /// <summary>
        ///  Khóa hoặc mở khóa tài khoản Shipper
        /// </summary>
        [HttpPut("shipper/{userId}/lock")]
        public async Task<IActionResult> LockUnlockShipper(string userId, [FromBody] LockRequest req)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            if (user.Role != "Shipper")
                return BadRequest(new { error = "User is not a Shipper" });

            // Cập nhật trạng thái trong DynamoDB
            await _userRepository.UpdateUserStatusAsync(userId, req.Unlock);

            // Cập nhật trong Cognito
            if (req.Unlock)
                await _authService.AdminEnableUserAsync(user.Username);
            else
                await _authService.AdminDisableUserAsync(user.Username);

            return Ok(new { message = req.Unlock ? " Shipper unlocked" : " Shipper locked" });
        }

        /// <summary>
        ///  Reset mật khẩu cho Shipper (Cognito sẽ gửi email)
        /// </summary>
        [HttpPost("shipper/{userId}/reset-password")]
        public async Task<IActionResult> ResetShipperPassword(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            if (user.Role != "Shipper")
                return BadRequest(new { error = "User is not a Shipper" });

            await _authService.AdminResetUserPasswordAsync(user.Username);

            return Ok(new
            {
                message = $" Password reset initiated for {user.Username}.",
                note = "Cognito will send an email to the user (if email is verified)."
            });
        }

        public class LockRequest
        {
            public bool Unlock { get; set; } = false; // false = lock, true = unlock
        }
    }
}
