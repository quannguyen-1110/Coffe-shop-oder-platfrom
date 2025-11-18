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
        private readonly ShipperProfileRepository _profileRepo; // ✅ THÊM

        public AdminController(
            UserRepository userRepository, 
            AuthService authService, 
            EmailService emailService, 
            OrderService orderService,
            ShipperProfileRepository profileRepo) // ✅ THÊM
        {
            _userRepository = userRepository;
            _authService = authService;
            _emailService = emailService;
            _orderService = orderService;
            _profileRepo = profileRepo; // ✅ THÊM
        }

        /// <summary>
        ///  Lấy danh sách Shipper đang chờ duyệt
        /// ✅ TRẢ VỀ data từ ShipperProfile
        /// </summary>
        [HttpGet("shippers/pending")]
        public async Task<IActionResult> GetPendingShippers()
        {
            try
 {
                var allShippers = await _userRepository.GetUsersByRoleAsync("Shipper");
         var pendingShipperIds = allShippers
                    .Where(s => s.RegistrationStatus == "Pending")
.Select(s => s.UserId)
      .ToList();

        // ✅ Lấy ShipperProfile để có đầy đủ thông tin
var profiles = new List<object>();
       
        foreach (var shipperId in pendingShipperIds)
   {
 var profile = await _profileRepo.GetProfileAsync(shipperId);
 var user = allShippers.First(s => s.UserId == shipperId);
   
    profiles.Add(new
    {
     shipperId = shipperId,
       username = user.Username,
    fullName = profile?.FullName ?? user.FullName ?? "Chưa cập nhật",
   email = profile?.Email ?? user.Email ?? user.Username,
       phone = profile?.Phone ?? user.PhoneNumber ?? "Chưa cập nhật",
   vehicleType = profile?.VehicleType ?? user.VehicleType ?? "Chưa cập nhật",
      vehiclePlate = profile?.VehiclePlate ?? user.LicensePlate ?? "Chưa cập nhật", // ✅ Đổi thành vehiclePlate
    registrationStatus = user.RegistrationStatus,
    createdAt = user.CreatedAt
});
 }

    return Ok(profiles);
   }
            catch (Exception ex)
            {
   return StatusCode(500, new { error = ex.Message });
   }
        }

        /// <summary>
        ///  Lấy danh sách tất cả Shipper đã được duyệt
        /// ✅ TRẢ VỀ data từ ShipperProfile (vì FE design theo Profile)
        /// </summary>
        [HttpGet("shippers")]
        public async Task<IActionResult> GetShippers()
        {
            try
 {
  // 1. Lấy danh sách shipper approved từ CoffeeShopUsers
    var allShippers = await _userRepository.GetUsersByRoleAsync("Shipper");
   var approvedShipperIds = allShippers
          .Where(s => s.RegistrationStatus == "Approved")
 .Select(s => s.UserId)
                  .ToList();

       // 2. ✅ Lấy ShipperProfile cho từng shipper (có đủ data)
        var profiles = new List<object>();
       
              foreach (var shipperId in approvedShipperIds)
                {
   var profile = await _profileRepo.GetProfileAsync(shipperId);
  var user = allShippers.First(s => s.UserId == shipperId);
             
             // ✅ Trả về Profile data (đầy đủ) + một số field từ User (status)
           profiles.Add(new
      {
           // Primary data từ ShipperProfile
           shipperId = shipperId,
      fullName = profile?.FullName ?? user.FullName ?? "Chưa cập nhật",
                email = profile?.Email ?? user.Email ?? user.Username,
          phone = profile?.Phone ?? user.PhoneNumber ?? "Chưa cập nhật",
      vehicleType = profile?.VehicleType ?? user.VehicleType ?? "Chưa cập nhật",
          vehiclePlate = profile?.VehiclePlate ?? user.LicensePlate ?? "Chưa cập nhật", // ✅ Đổi thành vehiclePlate
       
          // Banking info (chỉ có trong Profile)
                 bankAccount = profile?.BankAccount ?? "Chưa cập nhật",
         bankName = profile?.BankName ?? "Chưa cập nhật",
              
     // Stats (chỉ có trong Profile)
totalDeliveries = profile?.TotalDeliveries ?? 0,
             totalEarnings = profile?.TotalEarnings ?? 0,
      rating = profile?.Rating ?? 0,
   
     // Status info từ User table
isActive = user.IsActive,
       registrationStatus = user.RegistrationStatus,
           approvedAt = user.ApprovedAt,
            lastActiveAt = profile?.LastActiveAt
               });
                }

         return Ok(profiles);
 }
            catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
   }
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
