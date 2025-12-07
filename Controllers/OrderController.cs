using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        private readonly OrderService _orderService;
        private readonly MoMoService _momoService;

        public OrderController(
  OrderRepository orderRepository,
     OrderService orderService,
     MoMoService momoService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
            _momoService = momoService;
        }

        // ========== USER ENDPOINTS ==========

        /// <summary>
        /// 📋 User xem lịch sử đơn hàng của mình
        /// </summary>
        [HttpGet("my-orders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyOrderHistory()
        {
            try
            {
                // Lấy userId từ token
                var userId = User.FindFirstValue("sub") // Cognito sub claim
  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
  ?? User.FindFirstValue("cognito:username")
          ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                    return Unauthorized(new
                    {
                        error = "Cannot identify user from token",
                        availableClaims = claims,
                        hint = "Make sure you're using the ID token, not access token"
                    });
                }

                // Lấy danh sách đơn hàng của user
                var orders = await _orderService.GetUserOrdersAsync(userId);

                // Sắp xếp theo thời gian tạo (mới nhất trước)
                var orderedHistory = orders.OrderByDescending(o => o.CreatedAt).ToList();

                // Tạo response với thông tin tóm tắt
                var orderHistory = orderedHistory.Select(order => new
                {
                    orderId = order.OrderId,
                    status = order.Status,
                    totalPrice = order.TotalPrice,
                    finalPrice = order.FinalPrice,
                    appliedVoucherCode = order.AppliedVoucherCode,
                    discountAmount = order.TotalPrice - order.FinalPrice,
                    paymentMethod = order.PaymentMethod,
                    deliveryAddress = order.DeliveryAddress,
                    deliveryPhone = order.DeliveryPhone,
                    deliveryNote = order.DeliveryNote,
                    shippingFee = order.ShippingFee,

                    // Thông tin thời gian
                    createdAt = order.CreatedAt,
                    confirmedAt = order.ConfirmedAt,
                    shippingAt = order.ShippingAt,
                    deliveredAt = order.DeliveredAt,
                    completedAt = order.CompletedAt,

                    // Thông tin items (tóm tắt)
                    itemCount = order.Items?.Count ?? 0,
                    items = order.Items?.Select(item => new
                    {
                        productId = item.ProductId,
                        productType = item.ProductType,
                        productName = item.ProductName,
                        quantity = item.Quantity,
                        unitPrice = item.UnitPrice,
                        totalPrice = item.TotalPrice,
                        toppingCount = item.Toppings?.Count ?? 0
                    }).ToList(),

                    // Status display cho FE
                    statusDisplay = GetStatusDisplay(order.Status),
                    canCancel = CanCancelOrder(order.Status),
                    canReorder = CanReorderOrder(order.Status)
                }).ToList();

                return Ok(new
                {
                    message = "Order history retrieved successfully",
                    totalOrders = orderHistory.Count,
                    orders = orderHistory,
                    statistics = new
                    {
                        pendingOrders = orderHistory.Count(o => o.status == "Pending"),
                        processingOrders = orderHistory.Count(o => o.status == "Processing"),
                        confirmedOrders = orderHistory.Count(o => o.status == "Confirmed"),
                        shippingOrders = orderHistory.Count(o => o.status == "Shipping"),
                        deliveredOrders = orderHistory.Count(o => o.status == "Delivered"),
                        completedOrders = orderHistory.Count(o => o.status == "Completed"),
                        cancelledOrders = orderHistory.Count(o => o.status == "Cancelled"),
                        totalSpent = orderHistory.Where(o => o.status == "Completed").Sum(o => o.finalPrice)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 📄 User xem chi tiết 1 đơn hàng của mình
        /// </summary>
        [HttpGet("my-orders/{orderId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyOrderDetail(string orderId)
        {
            try
            {
                // Lấy userId từ token
                var userId = User.FindFirstValue("sub")
           ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? User.FindFirstValue("cognito:username")
                    ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Cannot identify user from token" });
                }

                // Lấy chi tiết đơn hàng
                var order = await _orderService.GetOrderAsync(orderId);

                if (order == null)
                    return NotFound(new { error = "Order not found" });

                // Kiểm tra quyền sở hữu
                if (order.UserId != userId)
                    return StatusCode(403, new { error = "You don't have permission to view this order" });

                // Trả về chi tiết đầy đủ
                return Ok(new
                {
                    message = "Order detail retrieved successfully",
                    order = new
                    {
                        orderId = order.OrderId,
                        userId = order.UserId,
                        status = order.Status,
                        statusDisplay = GetStatusDisplay(order.Status),

                        // Giá cả
                        totalPrice = order.TotalPrice,
                        finalPrice = order.FinalPrice,
                        discountAmount = order.TotalPrice - order.FinalPrice,
                        appliedVoucherCode = order.AppliedVoucherCode,
                        shippingFee = order.ShippingFee,
                        paymentMethod = order.PaymentMethod,

                        // Thông tin giao hàng
                        deliveryAddress = order.DeliveryAddress,
                        deliveryPhone = order.DeliveryPhone,
                        deliveryNote = order.DeliveryNote,
                        distanceKm = order.DistanceKm,

                        // Thời gian
                        createdAt = order.CreatedAt,
                        confirmedAt = order.ConfirmedAt,
                        shippingAt = order.ShippingAt,
                        deliveredAt = order.DeliveredAt,
                        completedAt = order.CompletedAt,

                        // Items chi tiết
                        items = order.Items?.Select(item => new
                        {
                            productId = item.ProductId,
                            productType = item.ProductType,
                            productName = item.ProductName,
                            quantity = item.Quantity,
                            unitPrice = item.UnitPrice,
                            totalPrice = item.TotalPrice,
                            toppings = item.Toppings?.Select(t => new
                            {
                                toppingId = t.ToppingId,
                                toppingName = t.Name, // ✅ Sử dụng Name thay vì ToppingName
                                price = t.Price
                            }).ToList()
                        }).ToList(),

                        // Actions cho FE
                        canCancel = CanCancelOrder(order.Status),
                        canReorder = CanReorderOrder(order.Status),
                        canRate = CanRateOrder(order.Status)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========== HELPER METHODS ==========

        private static string GetStatusDisplay(string status)
        {
            return status switch
            {
                "Pending" => "Chờ thanh toán",
                "Processing" => "Đang xử lý",
                "Confirmed" => "Đã xác nhận",
                "Shipping" => "Đang giao hàng",
                "Delivered" => "Đã giao hàng",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                _ => status
            };
        }

        private static bool CanCancelOrder(string status)
        {
            return status is "Pending" or "Processing" or "Confirmed";
        }

        private static bool CanReorderOrder(string status)
        {
            return status is "Delivered" or "Completed" or "Cancelled";
        }

        private static bool CanRateOrder(string status)
        {
            return status is "Delivered" or "Completed";
        }

        // ========== EXISTING ENDPOINTS ==========

        //  1. Lấy danh sách tất cả đơn hàng (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return Ok(orders);
        }

        //  2. Xem chi tiết 1 đơn hàng (User hoặc Admin)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null) return NotFound("Order not found");
            return Ok(order);
        }

        //  3. Cập nhật trạng thái đơn hàng (Admin only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateStatusRequest req)
        {
            try
            {
                var order = await _orderService.UpdateStatusAsync(id, req.Status);
                return Ok(new { message = $"Order {id} updated to {req.Status}", order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //  4. Tạo đơn hàng mới (User hoặc Admin đều được)
        [HttpPost]
        [Authorize] // Chỉ cần authenticated, không cần role cụ thể
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                // Thử nhiều cách lấy userId từ Cognito token
                var userId = User.FindFirstValue("sub") // Cognito sub claim
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("cognito:username")
                 ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    // Debug: show all claims
                    var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                    return Unauthorized(new
                    {
                        error = "Cannot identify user from token",
                        availableClaims = claims,
                        hint = "Make sure you're using the ID token, not access token"
                    });
                }

                // ✅ Check duplicate clientOrderId để tránh duplicate orders
                if (!string.IsNullOrEmpty(request.ClientOrderId))
                {
                    var existingOrder = await _orderRepository.GetOrderByClientIdAsync(request.ClientOrderId);
                    if (existingOrder != null)
                    {
                        return Ok(new
                        {
                            message = "Order already exists",
                            order = existingOrder,
                            isDuplicate = true
                        });
                    }
                }

                // Validate delivery address
                if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
                    return BadRequest(new { error = "Delivery address is required" });

                var order = new Order
                {
                    UserId = userId,
                    Items = new List<OrderItem>(),
                    DeliveryAddress = request.DeliveryAddress,
                    DeliveryPhone = request.DeliveryPhone,
                    DeliveryNote = request.DeliveryNote,
                    ClientOrderId = request.ClientOrderId,
                    PaymentMethod = request.PaymentMethod
                };

                // Convert request items to OrderItem
                foreach (var itemReq in request.Items)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = itemReq.ProductId,
                        ProductType = itemReq.ProductType,
                        Quantity = itemReq.Quantity,
                        Toppings = itemReq.ToppingIds?.Select(id => new OrderTopping { ToppingId = id }).ToList()
                    };
                    order.Items.Add(orderItem);
                }

                var created = await _orderService.CreateOrderAsync(order);

                // ✅ ÁP DỤNG VOUCHER NGAY SAU KHI TẠO ORDER (NẾU CÓ)
                if (!string.IsNullOrEmpty(request.VoucherCode))
                {
                    try
                    {
                        created = await _orderService.ApplyVoucherAsync(created.OrderId, request.VoucherCode);
                        Console.WriteLine($"✅ Voucher {request.VoucherCode} applied successfully. Final price: {created.FinalPrice}");
                    }
                    catch (Exception voucherEx)
                    {
                        // ⚠️ Log error nhưng không fail toàn bộ order
                        Console.WriteLine($"❌ Voucher application failed: {voucherEx.Message}");
                        // Có thể return warning cho FE
                    }
                }

                // ✅ XỬ LÝ THEO PAYMENT METHOD
                if (request.PaymentMethod == "MoMo")
                {
                    var orderInfo = $"Thanh toan don hang {created.OrderId}";

                    // ✅ SỬ DỤNG FINALPRICE (ĐÃ ÁP DỤNG VOUCHER)
                    var paymentResponse = await _momoService.CreatePaymentAsync(
                               created.OrderId,
                created.FinalPrice, // ✅ Giá sau khi áp voucher
                       orderInfo
                 );

                    return Ok(new
                    {
                        message = "Order created successfully",
                        order = created,
                        voucherApplied = !string.IsNullOrEmpty(request.VoucherCode) && !string.IsNullOrEmpty(created.AppliedVoucherCode),
                        appliedVoucherCode = created.AppliedVoucherCode,
                        discountAmount = created.TotalPrice - created.FinalPrice,
                        payment = new
                        {
                            success = paymentResponse.Success,
                            payUrl = paymentResponse.PayUrl,
                            qrCodeUrl = paymentResponse.QrCodeUrl,
                            deepLink = paymentResponse.DeepLink,
                            message = paymentResponse.Message
                        }
                    });
                }
                else if (request.PaymentMethod == "VNPay")
                {
                    // VNPay: Giữ Pending, FE sẽ gọi /api/Payment/vnpay/create để lấy payment URL
                    return Ok(new
                    {
                        message = "Order created successfully. Please proceed to payment.",
                        order = created,
                        voucherApplied = !string.IsNullOrEmpty(request.VoucherCode) && !string.IsNullOrEmpty(created.AppliedVoucherCode),
                        appliedVoucherCode = created.AppliedVoucherCode,
                        discountAmount = created.TotalPrice - created.FinalPrice,
                        payment = new
                        {
                            message = "Please call /api/Payment/vnpay/create to get payment URL",
                            requiresPayment = true
                        }
                    });
                }
                else if (request.PaymentMethod == "COD")
                {
                    // ✅ COD: Tự động chuyển sang Processing để shipper có thể nhận đơn
                    await _orderService.UpdateStatusAsync(created.OrderId, "Processing");
                    Console.WriteLine($"✅ COD Order {created.OrderId} automatically moved to Processing");

                    // Lấy lại order sau khi update status
                    created = await _orderRepository.GetOrderByIdAsync(created.OrderId);

                    return Ok(new
                    {
                        message = "Order created successfully with COD payment",
                        order = created,
                        voucherApplied = !string.IsNullOrEmpty(request.VoucherCode) && !string.IsNullOrEmpty(created.AppliedVoucherCode),
                        appliedVoucherCode = created.AppliedVoucherCode,
                        discountAmount = created.TotalPrice - created.FinalPrice,
                        payment = new
                        {
                            message = "COD - Payment on delivery",
                            requiresPayment = false,
                            status = "Processing"
                        }
                    });
                }
                else
                {
                    // Unknown payment method
                    return Ok(new
                    {
                        message = "Order created successfully",
                        order = created,
                        voucherApplied = !string.IsNullOrEmpty(request.VoucherCode) && !string.IsNullOrEmpty(created.AppliedVoucherCode),
                        appliedVoucherCode = created.AppliedVoucherCode,
                        discountAmount = created.TotalPrice - created.FinalPrice,
                        payment = new { message = $"Payment method: {request.PaymentMethod}" }
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public class UpdateStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        public class ApplyVoucherRequest
        {
            public string VoucherCode { get; set; } = string.Empty;
        }
    }
}
