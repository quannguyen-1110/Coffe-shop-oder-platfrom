using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly VNPayService _vnPayService;
        private readonly OrderRepository _orderRepository;
        private readonly OrderService _orderService;
        private readonly IConfiguration _configuration;
        public PaymentController(
            VNPayService vnPayService,
            OrderRepository orderRepository,
            OrderService orderService,
            IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _configuration = configuration;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay cho order
        /// </summary>
        [HttpPost("vnpay/create")]
        [Authorize]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                // Lấy order từ database
                var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
                if (order == null)
                    return NotFound(new { error = "Order not found" });

                // Kiểm tra order phải là Pending
                if (order.Status != "Pending")
                    return BadRequest(new { error = "Order must be in Pending status to make payment" });

                // Lấy IP address của client
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // Tạo payment URL (ReturnUrl lấy từ appsettings.json)
                var paymentUrl = _vnPayService.CreatePaymentUrl(order.OrderId, order.FinalPrice, ipAddress);

                return Ok(new VNPayPaymentResponse
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    Message = "Tạo URL thanh toán thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Callback từ VNPay sau khi thanh toán (Redirect về FE)
        /// </summary>
        [HttpGet("vnpay/callback")]
        public async Task<IActionResult> VNPayCallback()
        {
            try
            {
                Console.WriteLine("=== VNPay Callback Received ===");
                Console.WriteLine($"Query params: {Request.QueryString}");

                // Xử lý callback từ VNPay
                var response = _vnPayService.ProcessCallback(Request.Query);

                Console.WriteLine($"Payment Success: {response.Success}");
                Console.WriteLine($"Order ID: {response.OrderId}");
                Console.WriteLine($"Message: {response.Message}");

                // ✅ Debug Environment logic
                var environment = _configuration["Environment"] ?? "Development";
                var frontendUrlDev = _configuration["Frontend:Development"];
                var frontendUrlProd = _configuration["Frontend:Production"];

                Console.WriteLine($"🔍 Environment: {environment}");
                Console.WriteLine($"🔍 Frontend Development: {frontendUrlDev}");
                Console.WriteLine($"🔍 Frontend Production: {frontendUrlProd}");

                var frontendUrl = environment == "Production"
                    ? frontendUrlProd
                    : frontendUrlDev;

                Console.WriteLine($"🎯 Selected Frontend URL: {frontendUrl}");

                if (response.Success)
                {
                    // Cập nhật order status
                    var order = await _orderRepository.GetOrderByIdAsync(response.OrderId);
                    if (order != null && order.Status == "Pending")
                    {
                        await _orderService.UpdateStatusAsync(response.OrderId, "Processing");
                        Console.WriteLine("Order status updated to Processing");
                    }

                    var redirectUrl = $"{frontendUrl}/payment-success?orderId={response.OrderId}&amount={response.Amount}&transactionId={response.TransactionId}&bankCode={response.BankCode}";
                    Console.WriteLine($"🚀 Redirecting to: {redirectUrl}");

                    return Redirect(redirectUrl);
                }
                else
                {
                    var redirectUrl = $"{frontendUrl}/payment-failed?orderId={response.OrderId}&message={Uri.EscapeDataString(response.Message)}";
                    Console.WriteLine($"🚀 Redirecting to: {redirectUrl}");

                    return Redirect(redirectUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Callback error: {ex.Message}");
                var frontendUrl = _configuration["Frontend:Production"] ?? "https://main.d3djm3hylbiyyu.amplifyapp.com";
                Console.WriteLine($"🚀 Error redirect to: {frontendUrl}/payment-failed");

                return Redirect($"{frontendUrl}/payment-failed?message={Uri.EscapeDataString("Lỗi hệ thống")}");
            }
        }

        /// <summary>
        /// IPN (Instant Payment Notification) từ VNPay
        /// </summary>
        [HttpGet("vnpay/ipn")]
        public async Task<IActionResult> VNPayIPN()
        {
            try
            {
                Console.WriteLine("=== VNPay IPN Received ===");
                Console.WriteLine($"Query params: {Request.QueryString}");

                var response = _vnPayService.ProcessCallback(Request.Query);

                Console.WriteLine($"IPN Success: {response.Success}");
                Console.WriteLine($"Order ID: {response.OrderId}");

                if (response.Success)
                {
                    // Cập nhật order
                    var order = await _orderRepository.GetOrderByIdAsync(response.OrderId);
                    if (order != null && order.Status == "Pending")
                    {
                        await _orderService.UpdateStatusAsync(response.OrderId, "Processing");
                        Console.WriteLine("Order updated via IPN");
                    }

                    // Trả về response cho VNPay
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    Console.WriteLine($"IPN failed: {response.Message}");
                    return Ok(new { RspCode = "99", Message = response.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IPN error: {ex.Message}");
                return Ok(new { RspCode = "99", Message = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thanh toán của order
        /// </summary>
        [HttpGet("status/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(string orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound(new { error = "Order not found" });

                return Ok(new
                {
                    orderId = order.OrderId,
                    status = order.Status,
                    totalPrice = order.TotalPrice,
                    finalPrice = order.FinalPrice,
                    isPaid = order.Status != "Pending",
                    message = GetStatusMessage(order.Status)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private string GetStatusMessage(string status)
        {
            return status switch
            {
                "Pending" => "Chờ thanh toán",
                "Processing" => "Đã thanh toán, đang xử lý",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                _ => "Không xác định"
            };
        }

        public class CreatePaymentRequest
        {
            public string OrderId { get; set; } = string.Empty;
        }
    }
}
