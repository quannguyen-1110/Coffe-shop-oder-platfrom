using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("Orders")]
    public class Order
    {
        [DynamoDBHashKey("Id")]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty]
        public string UserId { get; set; } = string.Empty; // ID của user tạo order

        [DynamoDBProperty]
        public List<OrderItem> Items { get; set; } = new();

        [DynamoDBProperty]
        public decimal TotalPrice { get; set; }

        [DynamoDBProperty]
        public decimal FinalPrice { get; set; }

        [DynamoDBProperty]
        public string AppliedVoucherCode { get; set; } = string.Empty;

        // Status flow: Pending → Processing → Confirmed → Shipping → Delivered → Completed
        [DynamoDBProperty]
        public string Status { get; set; } = "Pending";

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty]
        public DateTime? CompletedAt { get; set; }

        // ========== SHIPPING FIELDS ==========
        [DynamoDBProperty]
        public string? ShipperId { get; set; } // ID của shipper nhận đơn

        [DynamoDBProperty]
        public string DeliveryAddress { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string? DeliveryPhone { get; set; }

        [DynamoDBProperty]
        public string? DeliveryNote { get; set; }

        [DynamoDBProperty]
        public decimal ShippingFee { get; set; } = 0;

        [DynamoDBProperty]
        public decimal DistanceKm { get; set; } = 0;

        [DynamoDBProperty]
        public DateTime? ConfirmedAt { get; set; } // Admin confirm

        [DynamoDBProperty]
        public string? ConfirmedBy { get; set; } // Admin UserId

        [DynamoDBProperty]
        public DateTime? ShippingAt { get; set; } // Shipper accept

        [DynamoDBProperty]
        public DateTime? DeliveredAt { get; set; } // Hoàn thành giao hàng
    }
}
