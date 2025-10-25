using BE.Models;

namespace BE.Repository
{
    public interface ICakeRepository
    {
        Task<List<Cake>> GetAllAsync();
        Task<Cake?> GetByIdAsync(string id);
        Task CreateAsync(Cake cake);
        Task UpdateAsync(string id, Cake cake);
        Task DeleteAsync(string id);
        Task DecreaseStockAsync(string id, int quantity);
    }
}
