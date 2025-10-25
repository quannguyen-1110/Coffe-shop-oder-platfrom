using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BE.Models
{
    public class OrderItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("productId")]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("productType")]
        public string ProductType { get; set; } = "Drink"; // Drink | Cake

        [BsonElement("productName")]
        public string ProductName { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("unitPrice")]
        public decimal UnitPrice { get; set; }

        [BsonElement("toppings")]
        [BsonIgnoreIfNull]
        public List<Topping>? Toppings { get; set; }

        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; }  // UnitPrice * Quantity + topping cost
    }
}
