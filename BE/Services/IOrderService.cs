using BE.Models;
using BE.Models.Dto;

namespace BE.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderDto dto);
        Task<List<Order>> GetOrdersAsync();
        Task<Order?> GetOrderByIdAsync(string id);
        Task UpdateStatusAsync(string id, string status);
    }
}
