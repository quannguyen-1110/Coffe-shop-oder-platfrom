using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoMoPaymentController : ControllerBase
    {
        private readonly MoMoService _momoService;
        private readonly OrderRepository _orderRepository;
        private readonly OrderService _orderService;
        private readonly IConfiguration _configuration;

        public MoMoPaymentController(
            MoMoService momoService,
            OrderRepository orderRepository,
            OrderService orderService,
            IConfiguration configuration)
        {
            _momoService = momoService;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _configuration = configuration;
        }

        /// <summary>
        /// Tạo payment URL MoMo cho order
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] CreateMoMoPaymentRequest request)
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

                // Tạo payment với MoMo
                var orderInfo = $"Thanh toan don hang {order.OrderId}";
                var response = await _momoService.CreatePaymentAsync(order.OrderId, order.FinalPrice, orderInfo);

                if (response.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        payUrl = response.PayUrl,
                        qrCodeUrl = response.QrCodeUrl,
                        deepLink = response.DeepLink,
                        message = response.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = response.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Callback từ MoMo sau khi thanh toán (ReturnUrl)
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            try
            {
                Console.WriteLine("=== MoMo Callback Received ===");
                Console.WriteLine($"Query params: {Request.QueryString}");

                var response = _momoService.ProcessCallback(Request.Query);

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
                    var order = await _orderRepository.GetOrderByIdAsync(response.OrderId);
                    if (order != null && order.Status == "Pending")
                    {
                        await _orderService.UpdateStatusAsync(response.OrderId, "Processing");
                    }

                    var redirectUrl = $"{frontendUrl}/payment-success?orderId={response.OrderId}&amount={response.Amount}&transactionId={response.TransactionId}";
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
                var frontendUrl = _configuration["Frontend:Production"] ?? "https://main.d3djm3hylbiyyu.amplifyapp.com";
                Console.WriteLine($"🚀 Error redirect to: {frontendUrl}/payment-failed");

                return Redirect($"{frontendUrl}/payment-failed?message={Uri.EscapeDataString("Lỗi hệ thống")}");
            }
        }

        /// <summary>
        /// IPN từ MoMo (NotifyUrl) - Server to server
        /// </summary>
        [HttpPost("ipn")]
        public async Task<IActionResult> IPN()
        {
            try
            {
                Console.WriteLine("=== MoMo IPN Received ===");
                Console.WriteLine($"Query params: {Request.QueryString}");

                var response = _momoService.ProcessCallback(Request.Query);

                if (response.Success)
                {
                    var order = await _orderRepository.GetOrderByIdAsync(response.OrderId);
                    if (order != null && order.Status == "Pending")
                    {
                        await _orderService.UpdateStatusAsync(response.OrderId, "Processing");
                        Console.WriteLine("Order updated via IPN");
                    }

                    return Ok(new { resultCode = 0, message = "Success" });
                }
                else
                {
                    return Ok(new { resultCode = 1, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IPN error: {ex.Message}");
                return Ok(new { resultCode = 1, message = ex.Message });
            }
        }

        /// <summary>
        /// Test callback thủ công (bỏ qua signature validation)
        /// </summary>
        [HttpPost("test-callback")]
        public async Task<IActionResult> TestCallback([FromBody] TestCallbackRequest request)
        {
            try
            {
                Console.WriteLine("=== MoMo Test Callback ===");
                Console.WriteLine($"Order ID: {request.OrderId}");
                Console.WriteLine($"Result Code: {request.ResultCode}");

                if (request.ResultCode == 0)
                {
                    // Cập nhật order status
                    var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
                    if (order != null && order.Status == "Pending")
                    {
                        await _orderService.UpdateStatusAsync(request.OrderId, "Processing");
                        Console.WriteLine("Order status updated to Processing");

                        return Ok(new
                        {
                            success = true,
                            message = "✅ Test callback thành công! Order đã chuyển sang Processing",
                            orderId = request.OrderId,
                            oldStatus = "Pending",
                            newStatus = "Processing"
                        });
                    }
                    else if (order == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "❌ Order không tồn tại",
                            orderId = request.OrderId
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            success = true,
                            message = $"⚠️ Order đã ở trạng thái {order.Status}",
                            orderId = request.OrderId,
                            currentStatus = order.Status
                        });
                    }
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "❌ Thanh toán thất bại (resultCode != 0)",
                        orderId = request.OrderId,
                        resultCode = request.ResultCode
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public class CreateMoMoPaymentRequest
        {
            public string OrderId { get; set; } = string.Empty;
        }

        public class TestCallbackRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public int ResultCode { get; set; } = 0; // 0 = success
        }
    }
}
