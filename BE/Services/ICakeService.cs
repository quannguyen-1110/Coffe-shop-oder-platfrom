using BE.Models;

namespace BE.Services
{
    public interface ICakeService
    {
        Task<List<Cake>> GetAllAsync();
        Task<Cake?> GetByIdAsync(string id);
        Task<Cake> CreateAsync(Cake cake);
        Task UpdateAsync(string id, Cake cake);
        Task DeleteAsync(string id);
        Task<bool> DecreaseStockAsync(string id, int quantity);
    }
}
