using BE.Models;

namespace BE.Repository
{
    public interface IDrinkRepository
    {
        Task<List<Drink>> GetAllAsync();
        Task<Drink?> GetByIdAsync(string id);
        Task CreateAsync(Drink drink);
        Task UpdateAsync(string id, Drink drink);
        Task DeleteAsync(string id);
        Task DecreaseStockAsync(string id, int quantity);
    }
}
