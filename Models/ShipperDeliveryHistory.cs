using Amazon.DynamoDBv2.DataModel;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("ShipperDeliveryHistory")]
    public class ShipperDeliveryHistory
    {
        [DynamoDBHashKey("HistoryId")]
        public string HistoryId { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty]
        public string ShipperId { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string OrderId { get; set; } = string.Empty;

        [DynamoDBProperty]
        public decimal ShippingFee { get; set; }

        [DynamoDBProperty]
        public decimal DistanceKm { get; set; }

        [DynamoDBProperty]
        public string PickupAddress { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string DeliveryAddress { get; set; } = string.Empty;

        [DynamoDBProperty]
        public DateTime AcceptedAt { get; set; }

        [DynamoDBProperty]
        public DateTime? DeliveredAt { get; set; }

        [DynamoDBProperty]
        public int DeliveryTimeMinutes { get; set; } // Thời gian giao hàng thực tế

        [DynamoDBProperty]
        public string Status { get; set; } = string.Empty; // Shipping, Delivered, Cancelled

        [DynamoDBProperty]
        public string? CancellationReason { get; set; }

        [DynamoDBProperty]
        public double? CustomerRating { get; set; } // Rating từ customer (1-5)

        [DynamoDBProperty]
        public string? CustomerFeedback { get; set; }

        [DynamoDBProperty]
        public string? Notes { get; set; }
    }
}
