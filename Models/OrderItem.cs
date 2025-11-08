using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

namespace CoffeeShopAPI.Models
{
    public class OrderItem
    {
        [DynamoDBProperty]
        public string ProductId { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string ProductName { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string ProductType { get; set; } = "Drink"; // Drink | Cake

        [DynamoDBProperty]
        public int Quantity { get; set; }

        [DynamoDBProperty]
        public decimal UnitPrice { get; set; }

        [DynamoDBProperty]
        public List<OrderTopping>? Toppings { get; set; } = new();

        [DynamoDBProperty]
        public decimal TotalPrice { get; set; } // UnitPrice * Quantity + toppings
    }
}
