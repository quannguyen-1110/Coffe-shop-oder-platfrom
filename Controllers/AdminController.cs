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
        private readonly EmailService _emailService;
        private readonly OrderService _orderService;

        public AdminController(UserRepository userRepository, AuthService authService, EmailService emailService, OrderService orderService)
        {
            _userRepository = userRepository;
            _authService = authService;
            _emailService = emailService;
            _orderService = orderService;
        }

        /// <summary>
        ///  Lấy danh sách Shipper đang chờ duyệt
        /// </summary>
        [HttpGet("shippers/pending")]
        public async Task<IActionResult> GetPendingShippers()
        {
            var allShippers = await _userRepository.GetUsersByRoleAsync("Shipper");
            var pending = allShippers.Where(s => s.RegistrationStatus == "Pending").ToList();
            return Ok(pending);
        }

        /// <summary>
        ///  Lấy danh sách tất cả Shipper đã được duyệt
        /// </summary>
        [HttpGet("shippers")]
        public async Task<IActionResult> GetShippers()
        {
            var allShippers = await _userRepository.GetUsersByRoleAsync("Shipper");
            var approved = allShippers.Where(s => s.RegistrationStatus == "Approved").ToList();
            return Ok(approved);
        }

        /// <summary>
        /// ✅ Duyệt đơn đăng ký Shipper
        /// </summary>
        [HttpPost("shipper/{userId}/approve")]
        public async Task<IActionResult> ApproveShipper(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            if (user.Role != "Shipper")
                return BadRequest(new { error = "User is not a Shipper" });

            if (user.RegistrationStatus == "Approved")
                return BadRequest(new { error = "Shipper already approved" });

            // Generate mật khẩu tạm thời
            var temporaryPassword = _authService.GenerateRandomPassword();
            var passwordHash = _authService.HashPassword(temporaryPassword);

            // Cập nhật user
            user.PasswordHash = passwordHash;
            user.RegistrationStatus = "Approved";
            user.IsActive = true;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedBy = User.FindFirst("sub")?.Value; // UserId của Admin

            await _userRepository.UpdateUserAsync(user);

            // ✉️ Gửi email thông báo
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendShipperApprovalEmailAsync(
                    user.Email,
                    user.FullName ?? user.Username,
                    user.Username,
                    temporaryPassword
                );
            }

            return Ok(new
            {
                message = $"Shipper {user.Username} approved successfully",
                email_sent = user.Email
            });
        }

        /// <summary>
        /// ❌ Từ chối đơn đăng ký Shipper
        /// </summary>
        [HttpPost("shipper/{userId}/reject")]
        public async Task<IActionResult> RejectShipper(string userId, [FromBody] RejectRequest req)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            if (user.Role != "Shipper")
                return BadRequest(new { error = "User is not a Shipper" });

            // Cập nhật trạng thái
            user.RegistrationStatus = "Rejected";
            user.IsActive = false;
            await _userRepository.UpdateUserAsync(user);

            // Gửi email thông báo
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendShipperRejectionEmailAsync(
                    user.Email,
                    user.FullName ?? user.Username,
                    req.Reason ?? "Thông tin chưa đầy đủ hoặc không phù hợp"
                );
            }

            return Ok(new { message = "Shipper registration rejected", email_sent = user.Email });
        }

        /// <summary>
        ///  Khóa hoặc mở khóa tài khoản Shipper (chỉ dành cho Shipper đã approved)
        /// </summary>
        [HttpPut("shipper/{userId}/lock")]
        public async Task<IActionResult> LockUnlockShipper(string userId, [FromBody] LockRequest req)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            if (user.Role != "Shipper")
                return BadRequest(new { error = "User is not a Shipper" });

            if (user.RegistrationStatus != "Approved")
                return BadRequest(new { error = "Can only lock/unlock approved shippers" });

            // Cập nhật trạng thái
            await _userRepository.UpdateUserStatusAsync(userId, req.Unlock);

            return Ok(new { message = req.Unlock ? "Shipper unlocked" : "Shipper locked" });
        }

        /// <summary>
        ///  Reset mật khẩu cho Shipper (gửi email mật khẩu mới)
        /// </summary>
        [HttpPost("shipper/{userId}/reset-password")]
        public async Task<IActionResult> ResetShipperPassword(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            if (user.Role != "Shipper")
                return BadRequest(new { error = "User is not a Shipper" });

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest(new { error = "User has no email registered" });

            // Generate mật khẩu mới
            var newPassword = _authService.GenerateRandomPassword();
            user.PasswordHash = _authService.HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);

            // Gửi email
            await _emailService.SendShipperApprovalEmailAsync(
                user.Email,
                user.FullName ?? user.Username,
                user.Username,
                newPassword
            );

            return Ok(new
            {
                message = $"Password reset for {user.Username}",
                email_sent = user.Email
            });
        }

        // ========== ORDER MANAGEMENT ==========

        /// <summary>
        /// Lấy danh sách đơn hàng chờ xác nhận (status = Processing)
        /// </summary>
        [HttpGet("orders/pending-confirm")]
        public async Task<IActionResult> GetPendingConfirmOrders()
        {
            try
            {
                var orders = await _orderService.GetPendingConfirmOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xác nhận đơn hàng (Processing → Confirmed)
        /// </summary>
        [HttpPost("orders/{orderId}/confirm")]
        public async Task<IActionResult> ConfirmOrder(string orderId)
        {
            try
            {
                // Debug: Show all claims
                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                Console.WriteLine("=== Admin Claims ===");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }

                var adminId = User.FindFirst("sub")?.Value 
                             ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new { 
                        error = "Invalid admin token",
                        availableClaims = claims,
                        hint = "Cannot find 'sub' claim in token"
                    });
                }

                var order = await _orderService.ConfirmOrderAsync(orderId, adminId);

                return Ok(new
                {
                    message = "Order confirmed successfully",
                    order = new
                    {
                        orderId = order.OrderId,
                        status = order.Status,
                        confirmedAt = order.ConfirmedAt,
                        confirmedBy = order.ConfirmedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tất cả đơn hàng (để quản lý)
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetPendingConfirmOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class LockRequest
        {
            public bool Unlock { get; set; } = false; // false = lock, true = unlock
        }

        public class RejectRequest
        {
            public string? Reason { get; set; }
        }
    }
}
