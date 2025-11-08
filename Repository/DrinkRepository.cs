using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Repository
{
    public class DrinkRepository
    {
        private readonly IDynamoDBContext _context;

        public DrinkRepository(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Drink?> GetDrinkByIdAsync(string id)
        {
            return await _context.LoadAsync<Drink>(id);
        }

        public async Task<List<Drink>> GetAllDrinksAsync()
        {
            return await _context.ScanAsync<Drink>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task AddDrinkAsync(Drink drink)
        {
            await _context.SaveAsync(drink);
        }

        public async Task UpdateDrinkAsync(Drink drink)
        {
            await _context.SaveAsync(drink);
        }

        public async Task DeleteDrinkAsync(string id)
        {
            await _context.DeleteAsync<Drink>(id);
        }
    }
}
