using BE.Models;
using BE.Repository;

namespace BE.Services
{
    public class ToppingService : IToppingService
    {
        private readonly IToppingRepository _repo;

        public ToppingService(IToppingRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Topping>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<Topping?> GetByIdAsync(string id) => await _repo.GetByIdAsync(id);

        public async Task<Topping> CreateAsync(Topping topping)
        {
            await _repo.CreateAsync(topping);
            return topping;
        }

        public async Task UpdateAsync(string id, Topping topping)
        {
            await _repo.UpdateAsync(id, topping);
        }

        public async Task DeleteAsync(string id)
        {
            await _repo.DeleteAsync(id);
        }

        public async Task<bool> DecreaseStockAsync(string id, int quantity)
        {
            var topping = await _repo.GetByIdAsync(id);
            if (topping == null || topping.Stock < quantity) return false;

            await _repo.DecreaseStockAsync(id, quantity);
            return true;
        }
    }
}
