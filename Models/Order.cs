using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("CoffeeShopOrders")]
    public class Order
    {
        [DynamoDBHashKey]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty]
        public string CustomerId { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string? ShipperId { get; set; }

        [DynamoDBProperty]
        public List<OrderItem> Items { get; set; } = new();

        [DynamoDBProperty]
        public decimal TotalPrice { get; set; }

        [DynamoDBProperty]
        public string Status { get; set; } = "Pending"; // Pending, Delivering, Completed, Canceled

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty]
        public DateTime? CompletedAt { get; set; }
    }

    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
