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
         
       // ✅ Chỉ tự động tạo MoMo payment nếu PaymentMethod = "MoMo"
                if (request.PaymentMethod == "MoMo")
  {
         var orderInfo = $"Thanh toan don hang {created.OrderId}";
    var paymentResponse = await _momoService.CreatePaymentAsync(created.OrderId, created.FinalPrice, orderInfo);
         
               return Ok(new 
                { 
       message = "Order created successfully", 
           order = created,
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
       else
       {
           return Ok(new
        {
          message = "Order created successfully",
               order = created,
    payment = new { message = $"Payment method: {request.PaymentMethod}" }
   });
      }
            }
            catch (Exception ex)
            {
        return BadRequest(new { error = ex.Message });
            }
        }

        //  5. Áp dụng voucher cho đơn hàng
   [HttpPost("{id}/apply-voucher")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ApplyVoucher(string id, [FromBody] ApplyVoucherRequest req)
        {
            try
        {
     var order = await _orderService.ApplyVoucherAsync(id, req.VoucherCode);
  return Ok(new { message = "Voucher applied successfully", order });
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
