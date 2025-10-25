using BE.Models;
using BE.Models.Dto;
using BE.Repository;

namespace BE.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICakeRepository _cakeRepo;
        private readonly IDrinkRepository _drinkRepo;
        private readonly IToppingRepository _toppingRepo;

        public OrderService(
            IOrderRepository orderRepo,
            ICakeRepository cakeRepo,
            IDrinkRepository drinkRepo,
            IToppingRepository toppingRepo)
        {
            _orderRepo = orderRepo;
            _cakeRepo = cakeRepo;
            _drinkRepo = drinkRepo;
            _toppingRepo = toppingRepo;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
        {
            decimal total = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                if (item.ProductType == "Drink")
                {
                    // Lấy thông tin nước uống
                    var drink = await _drinkRepo.GetByIdAsync(item.ProductId);
                    if (drink == null)
                        throw new Exception($"Drink not found: {item.ProductId}");

                    if (drink.Stock < item.Quantity)
                        throw new Exception($"{drink.Name} is out of stock");

                    decimal toppingCost = 0;

                    // Kiểm tra topping
                    if (item.Toppings != null && item.Toppings.Any())
                    {
                        foreach (var toppingItem in item.Toppings)
                        {
                            var topping = await _toppingRepo.GetByIdAsync(toppingItem.Id);
                            if (topping == null)
                                throw new Exception($"Topping not found: {toppingItem.Id}");

                            if (topping.Stock <= 0)
                                throw new Exception($"{topping.Name} is out of stock");

                            toppingCost += topping.Price;
                            await _toppingRepo.DecreaseStockAsync(topping.Id, 1);
                        }
                    }

                    // Tổng giá 1 sản phẩm
                    var drinkTotal = (drink.BasePrice + toppingCost) * item.Quantity;
                    total += drinkTotal;

                    // Trừ tồn kho nước uống
                    await _drinkRepo.DecreaseStockAsync(drink.Id, item.Quantity);

                    // Ghi snapshot
                    orderItems.Add(new OrderItem
                    {
                        ProductId = drink.Id,
                        ProductType = "Drink",
                        ProductName = drink.Name,
                        Quantity = item.Quantity,
                        UnitPrice = drink.BasePrice,
                        TotalPrice = drinkTotal,
                        Toppings = item.Toppings
                    });
                }
                else if (item.ProductType == "Cake")
                {
                    // Lấy thông tin bánh
                    var cake = await _cakeRepo.GetByIdAsync(item.ProductId);
                    if (cake == null)
                        throw new Exception($"Cake not found: {item.ProductId}");

                    if (cake.Stock < item.Quantity)
                        throw new Exception($"{cake.Name} is out of stock");

                    var cakeTotal = cake.Price * item.Quantity;
                    total += cakeTotal;

                    // Trừ tồn kho
                    await _cakeRepo.DecreaseStockAsync(cake.Id, item.Quantity);

                    // Ghi snapshot
                    orderItems.Add(new OrderItem
                    {
                        ProductId = cake.Id,
                        ProductType = "Cake",
                        ProductName = cake.Name,
                        Quantity = item.Quantity,
                        UnitPrice = cake.Price,
                        TotalPrice = cakeTotal
                    });
                }
                else
                {
                    throw new Exception($"Unknown product type: {item.ProductType}");
                }
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = dto.UserId,
                Items = orderItems,
                TotalPrice = total,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepo.CreateAsync(order);
            return order;
        }

        public async Task<List<Order>> GetOrdersAsync() => await _orderRepo.GetAllAsync();

        public async Task<Order?> GetOrderByIdAsync(string id) => await _orderRepo.GetByIdAsync(id);

        public async Task UpdateStatusAsync(string id, string status) =>
            await _orderRepo.UpdateStatusAsync(id, status);
    }
}
