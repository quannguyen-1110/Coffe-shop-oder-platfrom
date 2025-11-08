using Amazon.DynamoDBv2.DataModel;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("Drinks")]
    public class Drink
    {
        [DynamoDBHashKey("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [DynamoDBProperty("BasePrice")]
        public decimal BasePrice { get; set; }

        [DynamoDBProperty("Stock")]
        public int Stock { get; set; }

        [DynamoDBProperty("Category")]
        public string Category { get; set; } = "Default";

        [DynamoDBProperty("ImageUrl")]
        public string? ImageUrl { get; set; }
    }
}
