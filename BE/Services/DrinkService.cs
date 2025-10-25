using BE.Models;
using BE.Repository;

namespace BE.Services
{
    public class DrinkService : IDrinkService
    {
        private readonly IDrinkRepository _repo;

        public DrinkService(IDrinkRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Drink>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<Drink?> GetByIdAsync(string id) => await _repo.GetByIdAsync(id);

        public async Task<Drink> CreateAsync(Drink drink)
        {
            await _repo.CreateAsync(drink);
            return drink;
        }

        public async Task UpdateAsync(string id, Drink drink)
        {
            await _repo.UpdateAsync(id, drink);
        }

        public async Task DeleteAsync(string id)
        {
            await _repo.DeleteAsync(id);
        }

        public async Task<bool> DecreaseStockAsync(string id, int quantity)
        {
            var drink = await _repo.GetByIdAsync(id);
            if (drink == null || drink.Stock < quantity) return false;

            await _repo.DecreaseStockAsync(id, quantity);
            return true;
        }
    }
}
