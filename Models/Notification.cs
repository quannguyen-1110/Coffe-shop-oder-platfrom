using Amazon.DynamoDBv2.DataModel;
using System;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("Notifications")]
    public class Notification
    {
        [DynamoDBHashKey("NotificationId")]
        public string NotificationId { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Type { get; set; } = string.Empty; // OrderConfirmed, ShipperAccepted, OrderDelivered

        [DynamoDBProperty]
        public string Title { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Message { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string? OrderId { get; set; }

        [DynamoDBProperty]
        public string? ShipperId { get; set; }

        [DynamoDBProperty]
        public bool IsRead { get; set; } = false;

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty]
        public DateTime? ReadAt { get; set; }

        // Metadata for rich notifications
        [DynamoDBProperty]
        public string? Data { get; set; } // JSON string with additional data
    }
}
