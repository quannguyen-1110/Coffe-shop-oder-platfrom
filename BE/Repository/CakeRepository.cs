using BE.Data;
using BE.Models;
using MongoDB.Driver;

namespace BE.Repository
{
    public class CakeRepository : ICakeRepository
    {
        private readonly IMongoCollection<Cake> _cakes;

        public CakeRepository(MongoDbService mongoService)
        {
            _cakes = mongoService.GetCollection<Cake>("Cakes");
        }

        public async Task<List<Cake>> GetAllAsync() =>
            await _cakes.Find(_ => true).ToListAsync();

        public async Task<Cake?> GetByIdAsync(string id) =>
            await _cakes.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Cake cake) =>
            await _cakes.InsertOneAsync(cake);

        public async Task UpdateAsync(string id, Cake cake)
        {
            await _cakes.ReplaceOneAsync(c => c.Id == id, cake);
        }

        public async Task DeleteAsync(string id)
        {
            await _cakes.DeleteOneAsync(c => c.Id == id);
        }

        public async Task DecreaseStockAsync(string id, int quantity)
        {
            var update = Builders<Cake>.Update.Inc(c => c.Stock, -quantity);
            await _cakes.UpdateOneAsync(c => c.Id == id, update);
        }
    }
}
