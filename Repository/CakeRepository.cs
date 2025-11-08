using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Repository
{
    public class CakeRepository
    {
        private readonly IDynamoDBContext _context;

        public CakeRepository(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Cake?> GetCakeByIdAsync(string id)
        {
            return await _context.LoadAsync<Cake>(id);
        }

        public async Task<List<Cake>> GetAllCakesAsync()
        {
            return await _context.ScanAsync<Cake>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task AddCakeAsync(Cake cake)
        {
            await _context.SaveAsync(cake);
        }

        public async Task UpdateCakeAsync(Cake cake)
        {
            await _context.SaveAsync(cake);
        }

        public async Task DeleteCakeAsync(string id)
        {
            await _context.DeleteAsync<Cake>(id);
        }
    }
}
