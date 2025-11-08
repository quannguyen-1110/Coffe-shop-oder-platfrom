using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Repository
{
    public class ToppingRepository
    {
        private readonly IDynamoDBContext _context;

        public ToppingRepository(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Topping?> GetToppingByIdAsync(string id)
        {
            return await _context.LoadAsync<Topping>(id);
        }

        public async Task<List<Topping>> GetAllToppingsAsync()
        {
            return await _context.ScanAsync<Topping>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task AddToppingAsync(Topping topping)
        {
            await _context.SaveAsync(topping);
        }

        public async Task UpdateToppingAsync(Topping topping)
        {
            await _context.SaveAsync(topping);
        }

        public async Task DeleteToppingAsync(string id)
        {
            await _context.DeleteAsync<Topping>(id);
        }
    }
}
