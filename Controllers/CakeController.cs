using CoffeeShopAPI.Models;
using CoffeeShopAPI.Models.DTOs;
using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CakeController : ControllerBase
    {
        private readonly CakeRepository _cakeRepo;

        public CakeController(CakeRepository cakeRepo)
        {
            _cakeRepo = cakeRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCakes()
        {
            var cakes = await _cakeRepo.GetAllCakesAsync();
            return Ok(cakes);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCakeById(string id)
        {
            var cake = await _cakeRepo.GetCakeByIdAsync(id);
            if (cake == null)
                return NotFound(new { error = "Cake not found" });

            return Ok(cake);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCake([FromBody] Cake cake)
        {
            try
            {
                if (string.IsNullOrEmpty(cake.Name))
                    return BadRequest(new { error = "Name is required" });

                if (cake.Price <= 0)
                    return BadRequest(new { error = "Price must be greater than 0" });

                if (cake.Stock < 0)
                    return BadRequest(new { error = "Stock cannot be negative" });

                await _cakeRepo.AddCakeAsync(cake);
                return Ok(new { message = "Cake created successfully", cake });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCake(string id, [FromBody] UpdateCakeRequest request)
        {
            try
            {
                var cake = await _cakeRepo.GetCakeByIdAsync(id);
                if (cake == null)
                    return NotFound(new { error = "Cake not found" });

                if (!string.IsNullOrEmpty(request.Name))
                    cake.Name = request.Name;

                if (request.Price.HasValue)
                {
                    if (request.Price.Value <= 0)
                        return BadRequest(new { error = "Price must be greater than 0" });
                    cake.Price = request.Price.Value;
                }

                if (request.Stock.HasValue)
                {
                    if (request.Stock.Value < 0)
                        return BadRequest(new { error = "Stock cannot be negative" });
                    cake.Stock = request.Stock.Value;
                }

                if (request.ImageUrl != null)
                    cake.ImageUrl = request.ImageUrl;

                await _cakeRepo.UpdateCakeAsync(cake);
                return Ok(new { message = "Cake updated successfully", cake });
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
                var cake = await _cakeRepo.GetCakeByIdAsync(id);
                if (cake == null)
                    return NotFound(new { error = "Cake not found" });

                if (request.Stock < 0)
                    return BadRequest(new { error = "Stock cannot be negative" });

                cake.Stock = request.Stock;
                await _cakeRepo.UpdateCakeAsync(cake);

                return Ok(new 
                { 
                    message = "Stock updated successfully", 
                    cakeId = cake.Id,
                    cakeName = cake.Name,
                    newStock = cake.Stock 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCake(string id)
        {
            try
            {
                var cake = await _cakeRepo.GetCakeByIdAsync(id);
                if (cake == null)
                    return NotFound(new { error = "Cake not found" });

                await _cakeRepo.DeleteCakeAsync(id);
                return Ok(new { message = "Cake deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLowStockCakes([FromQuery] int threshold = 10)
        {
            var allCakes = await _cakeRepo.GetAllCakesAsync();
            var lowStockCakes = allCakes.Where(c => c.Stock < threshold).ToList();

            return Ok(new
            {
                threshold,
                count = lowStockCakes.Count,
                cakes = lowStockCakes
            });
        }

    }
}
