using BE.Models;

namespace BE.Repository
{
    public interface IToppingRepository
    {
        Task<List<Topping>> GetAllAsync();
        Task<Topping?> GetByIdAsync(string id);
        Task CreateAsync(Topping topping);
        Task UpdateAsync(string id, Topping topping);
        Task DeleteAsync(string id);
        Task DecreaseStockAsync(string id, int quantity);
    }
}
