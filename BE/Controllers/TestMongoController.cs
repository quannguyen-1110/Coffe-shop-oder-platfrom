using Microsoft.AspNetCore.Mvc;
using BE.Data;
using MongoDB.Bson;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestMongoController : ControllerBase
    {
        private readonly MongoDbService _mongoService;

        public TestMongoController(MongoDbService mongoService)
        {
            _mongoService = mongoService;
        }

        // Kiểm tra kết nối
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            try
            {
                _mongoService.GetCollection<BsonDocument>("TestCollection");
                return Ok("✅ Connected to MongoDB successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ MongoDB connection failed: {ex.Message}");
            }
        }

        // 🧩 API chèn dữ liệu mẫu
        [HttpPost("add-sample")]
        public IActionResult AddSample()
        {
            var collection = _mongoService.GetCollection<BsonDocument>("TestCollection");

            var sample = new BsonDocument
            {
                { "name", "Coffee Latte" },
                { "price", 45000 },
                { "createdAt", DateTime.Now }
            };

            collection.InsertOne(sample);

            return Ok("✅ Inserted sample document into MongoDB!");
        }
    }
}
