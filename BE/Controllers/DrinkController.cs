using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrinkController : ControllerBase
    {
        private readonly IDrinkService _service;

        public DrinkController(IDrinkService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var drink = await _service.GetByIdAsync(id);
            if (drink == null) return NotFound();
            return Ok(drink);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Drink drink)
        {
            await _service.CreateAsync(drink);
            return CreatedAtAction(nameof(GetById), new { id = drink.Id }, drink);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Drink drink)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            drink.Id = id;
            await _service.UpdateAsync(id, drink);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/decreaseStock/{quantity}")]
        public async Task<IActionResult> DecreaseStock(string id, int quantity)
        {
            var success = await _service.DecreaseStockAsync(id, quantity);
            if (!success) return BadRequest("Not enough stock");
            return NoContent();
        }
    }
}
