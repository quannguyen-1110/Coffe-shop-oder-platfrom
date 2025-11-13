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

        public PaymentController(
            VNPayService vnPayService,
            OrderRepository orderRepository,
            OrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
            _orderService = orderService;
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

                // Tạo payment request
                var paymentRequest = new VNPayPaymentRequest
                {
                    OrderId = order.OrderId,
                    Amount = order.FinalPrice,
                    OrderInfo = $"Thanh toan don hang {order.OrderId}",
                    ReturnUrl = request.ReturnUrl ?? "http://localhost:5144/api/payment/vnpay/callback"
                };

                // Tạo payment URL
                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentRequest, ipAddress);

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
        /// Callback từ VNPay sau khi thanh toán
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

                if (response.Success)
                {
                    // Cập nhật order status
                    var order = await _orderRepository.GetOrderByIdAsync(response.OrderId);
                    if (order != null)
                    {
                        Console.WriteLine($"Order found. Current status: {order.Status}");

                        // Chỉ cập nhật nếu order đang Pending
                        if (order.Status == "Pending")
                        {
                            await _orderService.UpdateStatusAsync(response.OrderId, "Processing");
                            Console.WriteLine("Order status updated to Processing");
                        }
                        else
                        {
                            Console.WriteLine($"Order already processed. Status: {order.Status}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Order not found!");
                    }

                    // ✅ REDIRECT VỀ FRONTEND SUCCESS PAGE
                    return Redirect($"http://localhost:3000/payment-result?orderId={response.OrderId}&status=success&amount={response.Amount}&transactionId={response.TransactionId}&bankCode={response.BankCode}&payDate={response.PayDate}");
                }
                else
                {
                    Console.WriteLine($"Payment failed: {response.Message}");

                    // ✅ REDIRECT VỀ FRONTEND ERROR PAGE  
                    return Redirect($"http://localhost:3000/payment-result?orderId={response.OrderId}&status=failed&message={Uri.EscapeDataString(response.Message)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // ✅ REDIRECT VỀ FRONTEND ERROR PAGE
                return Redirect($"http://localhost:3000/payment-result?status=error&message={Uri.EscapeDataString(ex.Message)}");
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
            public string? ReturnUrl { get; set; }
        }
    }
}
