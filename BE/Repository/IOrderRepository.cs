using BE.Models;

namespace BE.Repository
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(string id);
        Task CreateAsync(Order order);
        Task UpdateStatusAsync(string id, string status);
    }
}
