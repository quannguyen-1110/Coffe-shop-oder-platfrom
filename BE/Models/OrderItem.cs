using MongoDB.Bson.Serialization.Attributes;

namespace BE.Models

{
    public class OrderItem
    {
        [BsonElement("drinkName")]
        public string DrinkName { get; set; } = string.Empty;

        [BsonElement("size")]
        public string Size { get; set; } = "M";

        [BsonElement("toppings")]
        public List<Topping> Toppings { get; set; } = new();

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }
    }
}