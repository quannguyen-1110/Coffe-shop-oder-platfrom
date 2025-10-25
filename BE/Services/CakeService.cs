using BE.Models;
using BE.Repository;

namespace BE.Services
{
    public class CakeService : ICakeService
    {
        private readonly ICakeRepository _repo;

        public CakeService(ICakeRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Cake>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<Cake?> GetByIdAsync(string id) => await _repo.GetByIdAsync(id);

        public async Task<Cake> CreateAsync(Cake cake)
        {
            await _repo.CreateAsync(cake);
            return cake;
        }

        public async Task UpdateAsync(string id, Cake cake)
        {
            await _repo.UpdateAsync(id, cake);
        }

        public async Task DeleteAsync(string id)
        {
            await _repo.DeleteAsync(id);
        }

        public async Task<bool> DecreaseStockAsync(string id, int quantity)
        {
            var cake = await _repo.GetByIdAsync(id);
            if (cake == null || cake.Stock < quantity) return false;

            await _repo.DecreaseStockAsync(id, quantity);
            return true;
        }
    }
}
