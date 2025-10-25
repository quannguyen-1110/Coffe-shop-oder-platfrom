using BE.Data;
using BE.Models;
using MongoDB.Driver;

namespace BE.Repository
{
    public class DrinkRepository : IDrinkRepository
    {
        private readonly IMongoCollection<Drink> _drinks;

        public DrinkRepository(MongoDbService mongoService)
        {
            _drinks = mongoService.GetCollection<Drink>("Drinks");
        }

        public async Task<List<Drink>> GetAllAsync() =>
            await _drinks.Find(_ => true).ToListAsync();

        public async Task<Drink?> GetByIdAsync(string id) =>
            await _drinks.Find(d => d.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Drink drink) =>
            await _drinks.InsertOneAsync(drink);

        public async Task UpdateAsync(string id, Drink drink) =>
            await _drinks.ReplaceOneAsync(d => d.Id == id, drink);

        public async Task DeleteAsync(string id) =>
            await _drinks.DeleteOneAsync(d => d.Id == id);

        public async Task DecreaseStockAsync(string id, int quantity)
        {
            var update = Builders<Drink>.Update.Inc(d => d.Stock, -quantity);
            await _drinks.UpdateOneAsync(d => d.Id == id, update);
        }
    }
}
