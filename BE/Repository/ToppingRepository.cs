using BE.Data;
using BE.Models;
using MongoDB.Driver;

namespace BE.Repository
{
    public class ToppingRepository : IToppingRepository
    {
        private readonly IMongoCollection<Topping> _toppings;

        public ToppingRepository(MongoDbService mongoService)
        {
            _toppings = mongoService.GetCollection<Topping>("Toppings");
        }

        public async Task<List<Topping>> GetAllAsync() =>
            await _toppings.Find(_ => true).ToListAsync();

        public async Task<Topping?> GetByIdAsync(string id) =>
            await _toppings.Find(t => t.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Topping topping) =>
            await _toppings.InsertOneAsync(topping);

        public async Task UpdateAsync(string id, Topping topping) =>
            await _toppings.ReplaceOneAsync(t => t.Id == id, topping);

        public async Task DeleteAsync(string id) =>
            await _toppings.DeleteOneAsync(t => t.Id == id);

        public async Task DecreaseStockAsync(string id, int quantity)
        {
            var update = Builders<Topping>.Update.Inc(t => t.Stock, -quantity);
            await _toppings.UpdateOneAsync(t => t.Id == id, update);
        }
    }
}
