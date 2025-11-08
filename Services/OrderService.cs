using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;

namespace CoffeeShopAPI.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepo;
        private readonly LoyaltyService _loyaltyService;
        private readonly OrderItemService _orderItemService;

        public OrderService(OrderRepository orderRepo, LoyaltyService loyaltyService, OrderItemService orderItemService)
        {
            _orderRepo = orderRepo;
            _loyaltyService = loyaltyService;
            _orderItemService = orderItemService;
        }

        // ✅ Tạo đơn hàng với validation và tính giá tự động
        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.UserId))
                throw new Exception("UserId is required");

            if (order.Items == null || !order.Items.Any())
                throw new Exception("Order must have at least one item");

            // Validate và tính giá cho từng item
            decimal totalPrice = 0;
            foreach (var item in order.Items)
            {
                var validatedItem = await _orderItemService.ValidateAndCalculateItemAsync(item);
                totalPrice += validatedItem.TotalPrice;
            }

            order.TotalPrice = totalPrice;
            order.FinalPrice = totalPrice; // mặc định chưa giảm
            order.Status = "Pending";
            order.CreatedAt = DateTime.UtcNow;

            await _orderRepo.AddOrderAsync(order);
            return order;
        }

        // ✅ Áp dụng voucher trước khi xác nhận
        public async Task<Order> ApplyVoucherAsync(string orderId, string voucherCode)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != "Pending")
                throw new Exception("Cannot apply voucher to non-pending order");

            var discounted = await _loyaltyService.ApplyVoucherAsync(order.UserId, voucherCode, order.TotalPrice);

            order.AppliedVoucherCode = voucherCode;
            order.FinalPrice = discounted;

            await _orderRepo.UpdateOrderAsync(order);
            return order;
        }

        // ✅ Cập nhật trạng thái đơn hàng
        public async Task<Order> UpdateStatusAsync(string orderId, string status)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId)
                ?? throw new Exception("Order not found");

            var oldStatus = order.Status;
            order.Status = status;

            // ✅ Khi hoàn tất đơn → tự động cộng điểm và trừ stock (chỉ 1 lần)
            if (status == "Completed" && oldStatus != "Completed")
            {
                order.CompletedAt = DateTime.UtcNow;

                // Cộng điểm dựa trên FinalPrice (sau khi đã giảm giá)
                await _loyaltyService.AddPointsAsync(order.UserId, order.FinalPrice);

                // Trừ stock
                await _orderItemService.UpdateStockAfterOrderAsync(order.Items);
            }

            await _orderRepo.UpdateOrderAsync(order);
            return order;
        }

        public async Task<Order?> GetOrderAsync(string id)
        {
            return await _orderRepo.GetOrderByIdAsync(id);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _orderRepo.GetOrdersByUserAsync(userId);
        }
    }
}
