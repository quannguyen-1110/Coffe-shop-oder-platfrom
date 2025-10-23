using MongoDB.Bson.Serialization.Attributes;

namespace BE.Models

{
    public class Topping
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }
    }
}