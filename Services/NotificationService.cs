using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _notificationRepo;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IConfiguration _configuration;
        private readonly string? _topicArn;
        private readonly bool _isEnabled;

        public NotificationService(
            NotificationRepository notificationRepo,
            IConfiguration configuration)
        {
            _notificationRepo = notificationRepo;
            _configuration = configuration;
            _snsClient = new AmazonSimpleNotificationServiceClient();
            _topicArn = configuration["AWS:SNS:TopicArn"];
            _isEnabled = bool.Parse(configuration["Notifications:Enabled"] ?? "true");
        }

        /// <summary>
        /// Tạo và lưu notification vào database
        /// </summary>
        private async Task<Notification> CreateNotificationAsync(
            string userId,
            string type,
            string title,
            string message,
            string? orderId = null,
            string? shipperId = null,
            object? data = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                OrderId = orderId,
                ShipperId = shipperId,
                Data = data != null ? JsonSerializer.Serialize(data) : null,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddNotificationAsync(notification);
            Console.WriteLine($"[Notification] Created notification for user {userId}: {title}");

            return notification;
        }

        /// <summary>
        /// Publish notification to SNS Topic (optional - for real-time push)
        /// </summary>
        private async Task PublishToSNSAsync(Notification notification)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_topicArn))
            {
                Console.WriteLine($"[SNS] Disabled or no TopicArn configured");
                return;
            }

            try
            {
                var message = JsonSerializer.Serialize(new
                {
                    notification.NotificationId,
                    notification.UserId,
                    notification.Type,
                    notification.Title,
                    notification.Message,
                    notification.OrderId,
                    notification.CreatedAt
                });

                var request = new PublishRequest
                {
                    TopicArn = _topicArn,
                    Message = message,
                    Subject = notification.Title
                };

                await _snsClient.PublishAsync(request);
                Console.WriteLine($"[SNS] Published notification to topic");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SNS Error] Failed to publish: {ex.Message}");
                // Don't throw - SNS failure shouldn't break the flow
            }
        }

        /// <summary>
        /// Gửi thông báo khi admin confirm order
        /// </summary>
        public async Task NotifyOrderConfirmedAsync(Order order, User user)
        {
            var title = "Đơn hàng đã được xác nhận";
            var message = $"Đơn hàng #{order.OrderId.Substring(0, 8)} đã được xác nhận. Tổng tiền: {order.FinalPrice:N0}đ. Chúng tôi đang tìm shipper để giao hàng cho bạn.";

            var notification = await CreateNotificationAsync(
                userId: user.UserId,
                type: "OrderConfirmed",
                title: title,
                message: message,
                orderId: order.OrderId,
                data: new
                {
                    orderId = order.OrderId,
                    finalPrice = order.FinalPrice,
                    deliveryAddress = order.DeliveryAddress,
                    confirmedAt = order.ConfirmedAt
                }
            );

            await PublishToSNSAsync(notification);
        }

        /// <summary>
        /// Gửi thông báo khi shipper accept order
        /// </summary>
        public async Task NotifyShipperAcceptedAsync(Order order, User user, User shipper)
        {
            var title = "Shipper đã nhận đơn hàng";
            var message = $"Shipper {shipper.FullName ?? shipper.Username} đã nhận đơn hàng #{order.OrderId.Substring(0, 8)}. Khoảng cách: {order.DistanceKm:F1}km, Phí ship: {order.ShippingFee:N0}đ.";

            var notification = await CreateNotificationAsync(
                userId: user.UserId,
                type: "ShipperAccepted",
                title: title,
                message: message,
                orderId: order.OrderId,
                shipperId: shipper.UserId,
                data: new
                {
                    orderId = order.OrderId,
                    shipperName = shipper.FullName ?? shipper.Username,
                    shipperPhone = shipper.PhoneNumber,
                    vehicleType = shipper.VehicleType,
                    distanceKm = order.DistanceKm,
                    shippingFee = order.ShippingFee,
                    deliveryAddress = order.DeliveryAddress,
                    shippingAt = order.ShippingAt
                }
            );

            await PublishToSNSAsync(notification);
        }

        /// <summary>
        /// Gửi thông báo khi đang giao hàng
        /// </summary>
        public async Task NotifyShippingAsync(Order order, User user)
        {
            var title = "Đơn hàng đang được giao";
            var message = $"Đơn hàng #{order.OrderId.Substring(0, 8)} đang trên đường giao đến bạn! Vui lòng chuẩn bị nhận hàng.";

            var notification = await CreateNotificationAsync(
                userId: user.UserId,
                type: "OrderShipping",
                title: title,
                message: message,
                orderId: order.OrderId,
                data: new
                {
                    orderId = order.OrderId,
                    deliveryAddress = order.DeliveryAddress
                }
            );

            await PublishToSNSAsync(notification);
        }

        /// <summary>
        /// Gửi thông báo khi đã giao hàng thành công
        /// </summary>
        public async Task NotifyDeliveredAsync(Order order, User user)
        {
            var points = CalculatePoints(order.FinalPrice);
            var title = "Đơn hàng đã giao thành công";
            var message = $"Đơn hàng #{order.OrderId.Substring(0, 8)} đã được giao thành công! Bạn nhận được {points} điểm thưởng. Cảm ơn bạn đã mua hàng!";

            var notification = await CreateNotificationAsync(
                userId: user.UserId,
                type: "OrderDelivered",
                title: title,
                message: message,
                orderId: order.OrderId,
                data: new
                {
                    orderId = order.OrderId,
                    finalPrice = order.FinalPrice,
                    shippingFee = order.ShippingFee,
                    points = points,
                    deliveredAt = order.DeliveredAt
                }
            );

            await PublishToSNSAsync(notification);
        }

        /// <summary>
        /// Gửi thông báo cho shipper khi có đơn mới
        /// </summary>
        public async Task NotifyNewOrderForShippersAsync(Order order)
        {
            // TODO: Implement broadcast to all available shippers
            // For now, just log
            Console.WriteLine($"[Notification] New order available: {order.OrderId}");
        }

        // ========== HELPERS ==========

        private int CalculatePoints(decimal finalPrice)
        {
            // 1 điểm cho mỗi 10,000đ
            return (int)(finalPrice / 10000);
        }
    }
}
