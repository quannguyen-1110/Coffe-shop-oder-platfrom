using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BE.Models

{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("items")]
        public List<OrderItem> Items { get; set; } = new();

        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending | Confirmed | Completed | Cancelled

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}