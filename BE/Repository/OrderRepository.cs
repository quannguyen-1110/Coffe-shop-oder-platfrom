using BE.Data;
using BE.Models;
using MongoDB.Driver;

namespace BE.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orderCollection;

        public OrderRepository(MongoDbService mongoService)
        {
            _orderCollection = mongoService.GetCollection<Order>("Orders");
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _orderCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(string id)
        {
            return await _orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Order order)
        {
            await _orderCollection.InsertOneAsync(order);
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            var update = Builders<Order>.Update.Set(o => o.Status, status);
            await _orderCollection.UpdateOneAsync(o => o.Id == id, update);
        }
    }
}
