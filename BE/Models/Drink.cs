using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BE.Models
{
    public class Drink
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("basePrice")]
        public decimal BasePrice { get; set; }

        [BsonElement("stock")]
        public int Stock { get; set; } = 0;

        [BsonElement("category")]
        public string Category { get; set; } = "Default";

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }

    }
}
