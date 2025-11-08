using CoffeeShopAPI.Models;
using CoffeeShopAPI.Models.DTOs;
using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToppingController : ControllerBase
    {
        private readonly ToppingRepository _toppingRepo;

        public ToppingController(ToppingRepository toppingRepo)
        {
            _toppingRepo = toppingRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllToppings()
        {
            var toppings = await _toppingRepo.GetAllToppingsAsync();
            return Ok(toppings);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetToppingById(string id)
        {
            var topping = await _toppingRepo.GetToppingByIdAsync(id);
            if (topping == null)
                return NotFound(new { error = "Topping not found" });

            return Ok(topping);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTopping([FromBody] Topping topping)
        {
            try
            {
                if (string.IsNullOrEmpty(topping.Name))
                    return BadRequest(new { error = "Name is required" });

                if (topping.Price < 0)
                    return BadRequest(new { error = "Price cannot be negative" });

                if (topping.Stock < 0)
                    return BadRequest(new { error = "Stock cannot be negative" });

                await _toppingRepo.AddToppingAsync(topping);
                return Ok(new { message = "Topping created successfully", topping });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTopping(string id, [FromBody] UpdateToppingRequest request)
        {
            try
            {
                var topping = await _toppingRepo.GetToppingByIdAsync(id);
                if (topping == null)
                    return NotFound(new { error = "Topping not found" });

                if (!string.IsNullOrEmpty(request.Name))
                    topping.Name = request.Name;

                if (request.Price.HasValue)
                {
                    if (request.Price.Value < 0)
                        return BadRequest(new { error = "Price cannot be negative" });
                    topping.Price = request.Price.Value;
                }

                if (request.Stock.HasValue)
                {
                    if (request.Stock.Value < 0)
                        return BadRequest(new { error = "Stock cannot be negative" });
                    topping.Stock = request.Stock.Value;
                }

                if (request.ImageUrl != null)
                    topping.ImageUrl = request.ImageUrl;

                await _toppingRepo.UpdateToppingAsync(topping);
                return Ok(new { message = "Topping updated successfully", topping });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var topping = await _toppingRepo.GetToppingByIdAsync(id);
                if (topping == null)
                    return NotFound(new { error = "Topping not found" });

                if (request.Stock < 0)
                    return BadRequest(new { error = "Stock cannot be negative" });

                topping.Stock = request.Stock;
                await _toppingRepo.UpdateToppingAsync(topping);

                return Ok(new 
                { 
                    message = "Stock updated successfully", 
                    toppingId = topping.Id,
                    toppingName = topping.Name,
                    newStock = topping.Stock 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTopping(string id)
        {
            try
            {
                var topping = await _toppingRepo.GetToppingByIdAsync(id);
                if (topping == null)
                    return NotFound(new { error = "Topping not found" });

                await _toppingRepo.DeleteToppingAsync(id);
                return Ok(new { message = "Topping deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLowStockToppings([FromQuery] int threshold = 20)
        {
            var allToppings = await _toppingRepo.GetAllToppingsAsync();
            var lowStockToppings = allToppings.Where(t => t.Stock < threshold).ToList();

            return Ok(new
            {
                threshold,
                count = lowStockToppings.Count,
                toppings = lowStockToppings
            });
        }

    }
}
