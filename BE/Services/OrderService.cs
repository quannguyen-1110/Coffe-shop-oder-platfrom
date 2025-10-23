using BE.Models;
using BE.Models.Dto;
using BE.Repository;

namespace BE.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
        {
            var total = dto.Items.Sum(i => i.Price * i.Quantity);
            var order = new Order
            {
                UserId = dto.UserId,
                Items = dto.Items,
                TotalPrice = total
            };
            await _repo.CreateAsync(order);
            return order;
        }

        public async Task<List<Order>> GetOrdersAsync() => await _repo.GetAllAsync();

        public async Task<Order?> GetOrderByIdAsync(string id) => await _repo.GetByIdAsync(id);

        public async Task UpdateStatusAsync(string id, string status) => await _repo.UpdateStatusAsync(id, status);
    }
}
