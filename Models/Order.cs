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

        [DynamoDBProperty]
        public string Status { get; set; } = "Pending";

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty]
        public DateTime? CompletedAt { get; set; }
    }
}
