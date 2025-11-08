using CoffeeShopAPI.Models;
using CoffeeShopAPI.Models.DTOs;
using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrinkController : ControllerBase
    {
        private readonly DrinkRepository _drinkRepo;

        public DrinkController(DrinkRepository drinkRepo)
        {
            _drinkRepo = drinkRepo;
        }

        // Xem tất cả drinks (ai cũng xem được)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllDrinks()
        {
            var drinks = await _drinkRepo.GetAllDrinksAsync();
            return Ok(drinks);
        }

        // Xem chi tiết 1 drink
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDrinkById(string id)
        {
            var drink = await _drinkRepo.GetDrinkByIdAsync(id);
            if (drink == null)
                return NotFound(new { error = "Drink not found" });

            return Ok(drink);
        }

        // Admin tạo drink mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDrink([FromBody] Drink drink)
        {
            try
            {
                if (string.IsNullOrEmpty(drink.Name))
                    return BadRequest(new { error = "Name is required" });

                if (drink.BasePrice <= 0)
                    return BadRequest(new { error = "BasePrice must be greater than 0" });

                if (drink.Stock < 0)
                    return BadRequest(new { error = "Stock cannot be negative" });

                await _drinkRepo.AddDrinkAsync(drink);
                return Ok(new { message = "Drink created successfully", drink });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Admin cập nhật drink
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDrink(string id, [FromBody] UpdateDrinkRequest request)
        {
            try
            {
                var drink = await _drinkRepo.GetDrinkByIdAsync(id);
                if (drink == null)
                    return NotFound(new { error = "Drink not found" });

                if (!string.IsNullOrEmpty(request.Name))
                    drink.Name = request.Name;

                if (request.BasePrice.HasValue)
                {
                    if (request.BasePrice.Value <= 0)
                        return BadRequest(new { error = "BasePrice must be greater than 0" });
                    drink.BasePrice = request.BasePrice.Value;
                }

                if (request.Stock.HasValue)
                {
                    if (request.Stock.Value < 0)
                        return BadRequest(new { error = "Stock cannot be negative" });
                    drink.Stock = request.Stock.Value;
                }

                if (!string.IsNullOrEmpty(request.Category))
                    drink.Category = request.Category;

                if (request.ImageUrl != null)
                    drink.ImageUrl = request.ImageUrl;

                await _drinkRepo.UpdateDrinkAsync(drink);
                return Ok(new { message = "Drink updated successfully", drink });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Admin cập nhật stock (riêng biệt để dễ quản lý)
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var drink = await _drinkRepo.GetDrinkByIdAsync(id);
                if (drink == null)
                    return NotFound(new { error = "Drink not found" });

                if (request.Stock < 0)
                    return BadRequest(new { error = "Stock cannot be negative" });

                drink.Stock = request.Stock;
                await _drinkRepo.UpdateDrinkAsync(drink);

                return Ok(new 
                { 
                    message = "Stock updated successfully", 
                    drinkId = drink.Id,
                    drinkName = drink.Name,
                    newStock = drink.Stock 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Admin xóa drink
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDrink(string id)
        {
            try
            {
                var drink = await _drinkRepo.GetDrinkByIdAsync(id);
                if (drink == null)
                    return NotFound(new { error = "Drink not found" });

                await _drinkRepo.DeleteDrinkAsync(id);
                return Ok(new { message = "Drink deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Xem drinks sắp hết hàng (stock < threshold)
        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLowStockDrinks([FromQuery] int threshold = 10)
        {
            var allDrinks = await _drinkRepo.GetAllDrinksAsync();
            var lowStockDrinks = allDrinks.Where(d => d.Stock < threshold).ToList();

            return Ok(new
            {
                threshold,
                count = lowStockDrinks.Count,
                drinks = lowStockDrinks
            });
        }

    }
}
