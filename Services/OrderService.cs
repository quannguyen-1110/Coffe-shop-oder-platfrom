using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;

namespace CoffeeShopAPI.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepo;
        private readonly LoyaltyService _loyaltyService;
        private readonly OrderItemService _orderItemService;
        private readonly UserRepository _userRepo;
        private readonly NotificationService _notificationService;
        private readonly ShipperDeliveryHistoryRepository _historyRepo;
        private readonly ShipperProfileRepository _profileRepo;

        public OrderService(
            OrderRepository orderRepo, 
            LoyaltyService loyaltyService, 
            OrderItemService orderItemService,
            UserRepository userRepo,
            NotificationService notificationService,
            ShipperDeliveryHistoryRepository historyRepo,
            ShipperProfileRepository profileRepo)
        {
            _orderRepo = orderRepo;
            _loyaltyService = loyaltyService;
            _orderItemService = orderItemService;
            _userRepo = userRepo;
            _notificationService = notificationService;
            _historyRepo = historyRepo;
            _profileRepo = profileRepo;
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

        // ========== ADMIN OPERATIONS ==========

        /// <summary>
        /// Admin xác nhận đơn hàng (Processing → Confirmed)
        /// </summary>
        public async Task<Order> ConfirmOrderAsync(string orderId, string adminId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != "Processing")
                throw new Exception("Only Processing orders can be confirmed");

            order.Status = "Confirmed";
            order.ConfirmedAt = DateTime.UtcNow;
            order.ConfirmedBy = adminId;

            await _orderRepo.UpdateOrderAsync(order);

            // Gửi notification cho user
            var user = await _userRepo.GetUserByIdAsync(order.UserId);
            if (user != null)
            {
                await _notificationService.NotifyOrderConfirmedAsync(order, user);
            }

            // Thông báo cho shippers có đơn mới
            await _notificationService.NotifyNewOrderForShippersAsync(order);

            return order;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng chờ admin confirm
        /// </summary>
        public async Task<List<Order>> GetPendingConfirmOrdersAsync()
        {
            var allOrders = await _orderRepo.GetAllOrdersAsync();
            return allOrders.Where(o => o.Status == "Processing").OrderByDescending(o => o.CreatedAt).ToList();
        }

        // ========== SHIPPER OPERATIONS ==========

        /// <summary>
        /// Lấy danh sách đơn hàng available cho shipper (status = Confirmed)
        /// </summary>
        public async Task<List<Order>> GetAvailableOrdersForShipperAsync()
        {
            var allOrders = await _orderRepo.GetAllOrdersAsync();
            return allOrders.Where(o => o.Status == "Confirmed").OrderByDescending(o => o.ConfirmedAt).ToList();
        }

        /// <summary>
        /// Shipper chấp nhận đơn hàng (Confirmed → Shipping)
        /// </summary>
        public async Task<Order> AcceptOrderAsync(string orderId, string shipperId, decimal? shippingFee = null, decimal? distanceKm = null)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != "Confirmed")
                throw new Exception("Only Confirmed orders can be accepted");

            if (!string.IsNullOrEmpty(order.ShipperId))
                throw new Exception("Order already assigned to another shipper");

            order.Status = "Shipping";
            order.ShipperId = shipperId;
            order.ShippingAt = DateTime.UtcNow;
            
            // ✅ Update shipping fee nếu có
            if (shippingFee.HasValue)
                order.ShippingFee = shippingFee.Value;
            if (distanceKm.HasValue)
                order.DistanceKm = distanceKm.Value;

            await _orderRepo.UpdateOrderAsync(order);

            // ✅ Tạo delivery history
            var history = new ShipperDeliveryHistory
            {
                ShipperId = shipperId,
                OrderId = orderId,
                ShippingFee = order.ShippingFee,
                DistanceKm = order.DistanceKm,
                PickupAddress = "Coffee Shop", // TODO: Get from config
                DeliveryAddress = order.DeliveryAddress,
                AcceptedAt = DateTime.UtcNow,
                Status = "Shipping"
            };
            await _historyRepo.AddHistoryAsync(history);

            // Gửi notification cho user
            var user = await _userRepo.GetUserByIdAsync(order.UserId);
            var shipper = await _userRepo.GetUserByIdAsync(shipperId);
            if (user != null && shipper != null)
            {
                await _notificationService.NotifyShipperAcceptedAsync(order, user, shipper);
            }

            return order;
        }

        /// <summary>
        /// Shipper hoàn thành giao hàng (Shipping → Delivered)
        /// </summary>
        public async Task<Order> CompleteDeliveryAsync(string orderId, string shipperId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId)
                ?? throw new Exception("Order not found");

            if (order.ShipperId != shipperId)
                throw new Exception("You are not assigned to this order");

            if (order.Status != "Shipping")
                throw new Exception("Order is not in shipping status");

            order.Status = "Delivered";
            order.DeliveredAt = DateTime.UtcNow;

            await _orderRepo.UpdateOrderAsync(order);

            // ✅ Cập nhật delivery history
            var histories = await _historyRepo.GetOrderHistoryAsync(orderId);
            var history = histories.FirstOrDefault(h => h.ShipperId == shipperId && h.Status == "Shipping");
            if (history != null)
            {
                history.Status = "Delivered";
                history.DeliveredAt = DateTime.UtcNow;
                history.DeliveryTimeMinutes = (int)(DateTime.UtcNow - history.AcceptedAt).TotalMinutes;
                await _historyRepo.UpdateHistoryAsync(history);
            }

            // ✅ Cập nhật shipper profile
            var profile = await _profileRepo.GetProfileAsync(shipperId);
            if (profile != null)
            {
                profile.TotalDeliveries++;
                profile.TotalEarnings += order.ShippingFee;
                profile.LastActiveAt = DateTime.UtcNow;
                await _profileRepo.CreateOrUpdateProfileAsync(profile);
            }

            // Gửi notification cho user
            var user = await _userRepo.GetUserByIdAsync(order.UserId);
            if (user != null)
            {
                await _notificationService.NotifyDeliveredAsync(order, user);
            }

            return order;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của shipper
        /// </summary>
        public async Task<List<Order>> GetShipperOrdersAsync(string shipperId)
        {
            var allOrders = await _orderRepo.GetAllOrdersAsync();
            return allOrders.Where(o => o.ShipperId == shipperId).OrderByDescending(o => o.ShippingAt).ToList();
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
