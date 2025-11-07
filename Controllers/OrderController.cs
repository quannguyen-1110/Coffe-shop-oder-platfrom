using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] //  chỉ Admin & Staff có quyền quản lý đơn
    public class OrderController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        private readonly UserRepository _userRepository;

        public OrderController(OrderRepository orderRepository, UserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        //  1. Lấy danh sách tất cả đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return Ok(orders);
        }

        //  2. Xem chi tiết 1 đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null) return NotFound("Order not found");
            return Ok(order);
        }

        //  3. Cập nhật trạng thái đơn hàng
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateStatusRequest req)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null) return NotFound("Order not found");

            order.Status = req.Status;

            // Nếu hoàn tất → tự cộng điểm thưởng
            if (req.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                order.CompletedAt = DateTime.UtcNow;
                await RewardCustomerPoints(order);
            }

            await _orderRepository.UpdateOrderAsync(order);

            return Ok(new { message = $"Order {id} updated to {req.Status}" });
        }

        private async Task RewardCustomerPoints(Order order)
        {
            var customer = await _userRepository.GetUserByIdAsync(order.CustomerId);
            if (customer == null) return;

            // Cộng 1 điểm cho mỗi 10.000đ (tuỳ bạn chỉnh)
            int points = (int)(order.TotalPrice / 10000);
            customer.RewardPoints += points;
            await _userRepository.UpdateUserAsync(customer);
        }

        public class UpdateStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }
    }
}
