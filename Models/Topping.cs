using Amazon.DynamoDBv2.DataModel;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("Toppings")]
    public class Topping
    {
        [DynamoDBHashKey("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [DynamoDBProperty("Price")]
        public decimal Price { get; set; }

        [DynamoDBProperty("Stock")]
        public int Stock { get; set; }

        [DynamoDBProperty("ImageUrl")]
        public string? ImageUrl { get; set; }
    }
}
