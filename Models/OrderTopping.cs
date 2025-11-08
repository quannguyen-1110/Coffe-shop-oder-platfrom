using Amazon.DynamoDBv2.DataModel;

namespace CoffeeShopAPI.Models
{
    // Nested object trong OrderItem, KHÔNG phải table độc lập
    public class OrderTopping
    {
        [DynamoDBProperty("ToppingId")]
        public string ToppingId { get; set; } = string.Empty;

        [DynamoDBProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [DynamoDBProperty("Price")]
        public decimal Price { get; set; }
    }
}
