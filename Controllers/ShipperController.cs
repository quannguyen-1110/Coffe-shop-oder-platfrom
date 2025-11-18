using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShopAPI.Services;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Models;
using System.Security.Claims;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "ShipperAuth", Roles = "Shipper")]
    public class ShipperController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly ShippingService _shippingService;
        private readonly ShipperDeliveryHistoryRepository _historyRepo;
        private readonly ShipperProfileRepository _profileRepo;
        private readonly UserRepository _userRepo; // ✅ THÊM

        public ShipperController(
            OrderService orderService, 
            ShippingService shippingService,
            ShipperDeliveryHistoryRepository historyRepo,
            ShipperProfileRepository profileRepo,
            UserRepository userRepo) // ✅ THÊM
        {
            _orderService = orderService;
            _shippingService = shippingService;
            _historyRepo = historyRepo;
            _profileRepo = profileRepo;
            _userRepo = userRepo; // ✅ THÊM
        }

        /// <summary>
        /// Xem danh sách đơn hàng available (status = Confirmed)
        /// </summary>
        [HttpGet("orders/available")]
        public async Task<IActionResult> GetAvailableOrders()
        {
            try
            {
                var orders = await _orderService.GetAvailableOrdersForShipperAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xem chi tiết đơn hàng
        /// </summary>
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderDetail(string orderId)
        {
            try
            {
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                    return NotFound(new { error = "Order not found" });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Tính phí ship cho đơn hàng (trước khi accept)
        /// </summary>
        [HttpPost("orders/{orderId}/calculate-fee")]
        public async Task<IActionResult> CalculateShippingFee(string orderId)
        {
            try
            {
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                    return NotFound(new { error = "Order not found" });

                if (order.Status != "Confirmed")
                    return BadRequest(new { error = "Only Confirmed orders can calculate shipping fee" });

                if (string.IsNullOrEmpty(order.DeliveryAddress))
                    return BadRequest(new { error = "Order has no delivery address" });

                var calculation = await _shippingService.CalculateShippingAsync(order.DeliveryAddress);

                return Ok(new
                {
                    orderId = order.OrderId,
                    deliveryAddress = order.DeliveryAddress,
                    distanceKm = calculation.DistanceKm,
                    shippingFee = calculation.ShippingFee,
                    estimatedTime = calculation.EstimatedTime,
                    note = "Estimated delivery time in minutes"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Chấp nhận đơn hàng (Confirmed → Shipping)
        /// Tự động tính và lưu phí ship
        /// </summary>
        [HttpPost("orders/{orderId}/accept")]
        public async Task<IActionResult> AcceptOrder(string orderId)
        {
            try
            {
                var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(shipperId))
                    return Unauthorized(new { error = "Invalid shipper token" });

                // Tính phí ship trước khi accept
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                    return NotFound(new { error = "Order not found" });

                decimal shippingFee = 0;
                decimal distanceKm = 0;

                // ✅ Tính shipping fee
                if (!string.IsNullOrEmpty(order.DeliveryAddress))
                {
                    var calculation = await _shippingService.CalculateShippingAsync(order.DeliveryAddress);
                    distanceKm = calculation.DistanceKm;
                    shippingFee = calculation.ShippingFee;
                }

                // Accept order với shipping fee
                var acceptedOrder = await _orderService.AcceptOrderAsync(orderId, shipperId, shippingFee, distanceKm);

                return Ok(new
                {
                    message = "Order accepted successfully",
                    order = new
                    {
                        orderId = acceptedOrder.OrderId,
                        status = acceptedOrder.Status,
                        shipperId = acceptedOrder.ShipperId,
                        shippingAt = acceptedOrder.ShippingAt,
                        deliveryAddress = acceptedOrder.DeliveryAddress,
                        deliveryPhone = acceptedOrder.DeliveryPhone,
                        deliveryNote = acceptedOrder.DeliveryNote,
                        distanceKm = acceptedOrder.DistanceKm,
                        shippingFee = acceptedOrder.ShippingFee
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Hoàn thành giao hàng (Shipping → Delivered)
        /// </summary>
        [HttpPost("orders/{orderId}/complete")]
        public async Task<IActionResult> CompleteDelivery(string orderId)
        {
            try
            {
                var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(shipperId))
                    return Unauthorized(new { error = "Invalid shipper token" });

                var order = await _orderService.CompleteDeliveryAsync(orderId, shipperId);

                return Ok(new
                {
                    message = "Delivery completed successfully",
                    order = new
                    {
                        orderId = order.OrderId,
                        status = order.Status,
                        deliveredAt = order.DeliveredAt
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xem lịch sử đơn hàng của shipper
        /// </summary>
        [HttpGet("orders/history")]
        public async Task<IActionResult> GetOrderHistory()
        {
            try
            {
                var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(shipperId))
                    return Unauthorized(new { error = "Invalid shipper token" });

                var orders = await _orderService.GetShipperOrdersAsync(shipperId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xem thống kê của shipper
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(shipperId))
                    return Unauthorized(new { error = "Invalid shipper token" });

                var orders = await _orderService.GetShipperOrdersAsync(shipperId);

                var stats = new
                {
                    totalOrders = orders.Count,
                    completedOrders = orders.Count(o => o.Status == "Delivered" || o.Status == "Completed"),
                    shippingOrders = orders.Count(o => o.Status == "Shipping"),
                    totalEarnings = orders.Where(o => o.Status == "Delivered" || o.Status == "Completed").Sum(o => o.ShippingFee),
                    todayOrders = orders.Count(o => o.ShippingAt?.Date == DateTime.UtcNow.Date)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xem lịch sử giao hàng của shipper
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetDeliveryHistory()
        {
            try
            {
                var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(shipperId))
                    return Unauthorized(new { error = "Invalid shipper token" });

                var history = await _historyRepo.GetShipperHistoryAsync(shipperId);
                return Ok(history.OrderByDescending(h => h.AcceptedAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xem profile của shipper
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(shipperId))
                    return Unauthorized(new { error = "Invalid shipper token" });

                var profile = await _profileRepo.GetProfileAsync(shipperId);
                if (profile == null)
                    return NotFound(new { error = "Profile not found" });

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật profile của shipper
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(shipperId))
                    return Unauthorized(new { error = "Invalid shipper token" });

                // 1. ✅ Cập nhật ShipperProfile (bảng chính)
                var profile = await _profileRepo.GetProfileAsync(shipperId);
                if (profile == null)
                {
                    // Tạo profile mới
                    profile = new ShipperProfile
                    {
                        ShipperId = shipperId
                    };
                }

                // Update ShipperProfile fields
                if (!string.IsNullOrEmpty(request.FullName))
                    profile.FullName = request.FullName;
                if (!string.IsNullOrEmpty(request.Phone))
                    profile.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.VehicleType))
                    profile.VehicleType = request.VehicleType;
                if (!string.IsNullOrEmpty(request.VehiclePlate))
                    profile.VehiclePlate = request.VehiclePlate;
                if (!string.IsNullOrEmpty(request.BankAccount))
                    profile.BankAccount = request.BankAccount;
                if (!string.IsNullOrEmpty(request.BankName))
                    profile.BankName = request.BankName;

                profile.LastActiveAt = DateTime.UtcNow;

                await _profileRepo.CreateOrUpdateProfileAsync(profile);

                // 2. ✅ ĐỒNG BỘ sang CoffeeShopUsers (CHỈ các field CHUNG)
                var user = await _userRepo.GetUserByIdAsync(shipperId);
                if (user != null && user.Role == "Shipper") // ⚠️ KIỂM TRA ROLE
                {
                    // ⚠️ CHỈ sync 4 fields CHUNG, KHÔNG động:
                    // - RewardPoints, VoucherCount (dành cho User)
                    // - RegistrationStatus, IsActive (dành cho Admin)
                    // - PasswordHash, Username, Email (authentication)
                    
                    if (!string.IsNullOrEmpty(request.FullName))
                        user.FullName = request.FullName;
                    if (!string.IsNullOrEmpty(request.Phone))
                        user.PhoneNumber = request.Phone;
                    if (!string.IsNullOrEmpty(request.VehicleType))
                        user.VehicleType = request.VehicleType;
                    if (!string.IsNullOrEmpty(request.VehiclePlate))
                        user.LicensePlate = request.VehiclePlate;
                    
                    // ⚠️ KHÔNG update các field khác:
                    // user.RewardPoints - GIỮ NGUYÊN
                    // user.VoucherCount - GIỮ NGUYÊN
                    // user.RegistrationStatus - GIỮ NGUYÊN
                    // user.IsActive - GIỮ NGUYÊN
                    
                    await _userRepo.UpdateUserAsync(user);
                }

                return Ok(new
                {
                    message = "Profile updated successfully (synced to both tables)",
                    profile = new
                    {
                        shipperId = profile.ShipperId,
                        fullName = profile.FullName,
                        phone = profile.Phone,
                        vehicleType = profile.VehicleType,
                        vehiclePlate = profile.VehiclePlate,
                        bankAccount = profile.BankAccount,
                        bankName = profile.BankName,
                        totalEarnings = profile.TotalEarnings,
                        totalDeliveries = profile.TotalDeliveries,
                        rating = profile.Rating
                    },
                    syncedToUser = user != null && user.Role == "Shipper"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class UpdateProfileRequest
        {
            public string? FullName { get; set; }
            public string? Phone { get; set; }
            public string? VehicleType { get; set; }
            public string? VehiclePlate { get; set; }
            public string? BankAccount { get; set; }
            public string? BankName { get; set; }
        }
    }
}
