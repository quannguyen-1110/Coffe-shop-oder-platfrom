using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class InventoryController : ControllerBase
    {
        private readonly DrinkRepository _drinkRepo;
        private readonly CakeRepository _cakeRepo;
        private readonly ToppingRepository _toppingRepo;

        public InventoryController(
            DrinkRepository drinkRepo,
            CakeRepository cakeRepo,
            ToppingRepository toppingRepo)
        {
            _drinkRepo = drinkRepo;
            _cakeRepo = cakeRepo;
            _toppingRepo = toppingRepo;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetInventoryOverview()
        {
            var drinks = await _drinkRepo.GetAllDrinksAsync();
            var cakes = await _cakeRepo.GetAllCakesAsync();
            var toppings = await _toppingRepo.GetAllToppingsAsync();

            return Ok(new
            {
                drinks = new
                {
                    total = drinks.Count,
                    inStock = drinks.Count(d => d.Stock > 0),
                    outOfStock = drinks.Count(d => d.Stock == 0),
                    lowStock = drinks.Count(d => d.Stock > 0 && d.Stock < 10),
                    totalValue = drinks.Sum(d => d.BasePrice * d.Stock)
                },
                cakes = new
                {
                    total = cakes.Count,
                    inStock = cakes.Count(c => c.Stock > 0),
                    outOfStock = cakes.Count(c => c.Stock == 0),
                    lowStock = cakes.Count(c => c.Stock > 0 && c.Stock < 10),
                    totalValue = cakes.Sum(c => c.Price * c.Stock)
                },
                toppings = new
                {
                    total = toppings.Count,
                    inStock = toppings.Count(t => t.Stock > 0),
                    outOfStock = toppings.Count(t => t.Stock == 0),
                    lowStock = toppings.Count(t => t.Stock > 0 && t.Stock < 20),
                    totalValue = toppings.Sum(t => t.Price * t.Stock)
                }
            });
        }

        [HttpGet("alerts")]
        public async Task<IActionResult> GetStockAlerts()
        {
            var drinks = await _drinkRepo.GetAllDrinksAsync();
            var cakes = await _cakeRepo.GetAllCakesAsync();
            var toppings = await _toppingRepo.GetAllToppingsAsync();

            var alerts = new List<object>();

            // Out of stock items
            alerts.AddRange(drinks.Where(d => d.Stock == 0).Select(d => new
            {
                type = "Drink",
                id = d.Id,
                name = d.Name,
                stock = d.Stock,
                severity = "critical",
                message = $"{d.Name} is out of stock"
            }));

            alerts.AddRange(cakes.Where(c => c.Stock == 0).Select(c => new
            {
                type = "Cake",
                id = c.Id,
                name = c.Name,
                stock = c.Stock,
                severity = "critical",
                message = $"{c.Name} is out of stock"
            }));

            alerts.AddRange(toppings.Where(t => t.Stock == 0).Select(t => new
            {
                type = "Topping",
                id = t.Id,
                name = t.Name,
                stock = t.Stock,
                severity = "critical",
                message = $"{t.Name} is out of stock"
            }));

            // Low stock items
            alerts.AddRange(drinks.Where(d => d.Stock > 0 && d.Stock < 10).Select(d => new
            {
                type = "Drink",
                id = d.Id,
                name = d.Name,
                stock = d.Stock,
                severity = "warning",
                message = $"{d.Name} is running low (only {d.Stock} left)"
            }));

            alerts.AddRange(cakes.Where(c => c.Stock > 0 && c.Stock < 10).Select(c => new
            {
                type = "Cake",
                id = c.Id,
                name = c.Name,
                stock = c.Stock,
                severity = "warning",
                message = $"{c.Name} is running low (only {c.Stock} left)"
            }));

            alerts.AddRange(toppings.Where(t => t.Stock > 0 && t.Stock < 20).Select(t => new
            {
                type = "Topping",
                id = t.Id,
                name = t.Name,
                stock = t.Stock,
                severity = "warning",
                message = $"{t.Name} is running low (only {t.Stock} left)"
            }));

            return Ok(new
            {
                totalAlerts = alerts.Count,
                critical = alerts.Count(a => ((dynamic)a).severity == "critical"),
                warnings = alerts.Count(a => ((dynamic)a).severity == "warning"),
                alerts
            });
        }
    }
}
