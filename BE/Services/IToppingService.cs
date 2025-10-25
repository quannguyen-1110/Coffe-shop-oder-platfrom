using BE.Models;

namespace BE.Services
{
    public interface IToppingService
    {
        Task<List<Topping>> GetAllAsync();
        Task<Topping?> GetByIdAsync(string id);
        Task<Topping> CreateAsync(Topping topping);
        Task UpdateAsync(string id, Topping topping);
        Task DeleteAsync(string id);
        Task<bool> DecreaseStockAsync(string id, int quantity);
    }
}
