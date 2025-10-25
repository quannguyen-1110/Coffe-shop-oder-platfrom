using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BE.Models
{
    public class Cake
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("stock")]
        public int Stock { get; set; } = 0;

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }
    }
}
