using BE.Models;

namespace BE.Services
{
    public interface IDrinkService
    {
        Task<List<Drink>> GetAllAsync();
        Task<Drink?> GetByIdAsync(string id);
        Task<Drink> CreateAsync(Drink drink);
        Task UpdateAsync(string id, Drink drink);
        Task DeleteAsync(string id);
        Task<bool> DecreaseStockAsync(string id, int quantity);
    }
}
