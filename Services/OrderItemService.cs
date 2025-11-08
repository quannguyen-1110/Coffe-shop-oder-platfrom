using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;

namespace CoffeeShopAPI.Services
{
    public class OrderItemService
    {
        private readonly DrinkRepository _drinkRepo;
        private readonly CakeRepository _cakeRepo;
        private readonly ToppingRepository _toppingRepo;

        public OrderItemService(
            DrinkRepository drinkRepo,
            CakeRepository cakeRepo,
            ToppingRepository toppingRepo)
        {
            _drinkRepo = drinkRepo;
            _cakeRepo = cakeRepo;
            _toppingRepo = toppingRepo;
        }

        // ✅ Validate và tính giá cho OrderItem
        public async Task<OrderItem> ValidateAndCalculateItemAsync(OrderItem item)
        {
            if (string.IsNullOrEmpty(item.ProductId))
                throw new Exception("ProductId is required");

            if (item.Quantity <= 0)
                throw new Exception("Quantity must be greater than 0");

            // Lấy thông tin sản phẩm thực tế và kiểm tra stock
            if (item.ProductType == "Drink")
            {
                var drink = await _drinkRepo.GetDrinkByIdAsync(item.ProductId)
                    ?? throw new Exception("Drink not found");
                
                if (drink.Stock < item.Quantity)
                    throw new Exception($"Not enough stock for {drink.Name}. Available: {drink.Stock}");

                item.ProductName = drink.Name;
                item.UnitPrice = drink.BasePrice;
            }
            else if (item.ProductType == "Cake")
            {
                var cake = await _cakeRepo.GetCakeByIdAsync(item.ProductId)
                    ?? throw new Exception("Cake not found");
                
                if (cake.Stock < item.Quantity)
                    throw new Exception($"Not enough stock for {cake.Name}. Available: {cake.Stock}");

                item.ProductName = cake.Name;
                item.UnitPrice = cake.Price;
            }
            else
            {
                throw new Exception("Invalid ProductType. Must be 'Drink' or 'Cake'");
            }

            // Tính tổng topping và validate stock
            decimal toppingTotal = 0;
            if (item.Toppings != null && item.Toppings.Any())
            {
                foreach (var topping in item.Toppings)
                {
                    var toppingData = await _toppingRepo.GetToppingByIdAsync(topping.ToppingId)
                        ?? throw new Exception($"Topping {topping.ToppingId} not found");
                    
                    if (toppingData.Stock < item.Quantity)
                        throw new Exception($"Not enough stock for topping {toppingData.Name}");

                    topping.Name = toppingData.Name;
                    topping.Price = toppingData.Price;
                    toppingTotal += toppingData.Price;
                }
            }

            // Tổng tiền item
            item.TotalPrice = (item.UnitPrice + toppingTotal) * item.Quantity;

            return item;
        }

        // ✅ Cập nhật stock sau khi order completed
        public async Task UpdateStockAfterOrderAsync(List<OrderItem> items)
        {
            foreach (var item in items)
            {
                if (item.ProductType == "Drink")
                {
                    var drink = await _drinkRepo.GetDrinkByIdAsync(item.ProductId);
                    if (drink != null)
                    {
                        drink.Stock -= item.Quantity;
                        await _drinkRepo.UpdateDrinkAsync(drink);
                    }
                }
                else if (item.ProductType == "Cake")
                {
                    var cake = await _cakeRepo.GetCakeByIdAsync(item.ProductId);
                    if (cake != null)
                    {
                        cake.Stock -= item.Quantity;
                        await _cakeRepo.UpdateCakeAsync(cake);
                    }
                }

                // Cập nhật stock cho toppings
                if (item.Toppings != null)
                {
                    foreach (var topping in item.Toppings)
                    {
                        var toppingData = await _toppingRepo.GetToppingByIdAsync(topping.ToppingId);
                        if (toppingData != null)
                        {
                            toppingData.Stock -= item.Quantity;
                            await _toppingRepo.UpdateToppingAsync(toppingData);
                        }
                    }
                }
            }
        }
    }
}
