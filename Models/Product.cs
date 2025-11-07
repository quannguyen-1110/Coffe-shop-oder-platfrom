using Amazon.DynamoDBv2.DataModel;
using System;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("CoffeeShopProducts")]
    public class Product
    {
        [DynamoDBHashKey] // Khóa chính trong DynamoDB
        public string ProductId { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty]
        public string Name { get; set; } = string.Empty;

        [DynamoDBProperty]
        public decimal Price { get; set; }

        [DynamoDBProperty]
        public string Description { get; set; } = string.Empty;

        [DynamoDBProperty]
        public bool IsAvailable { get; set; } = true;

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
