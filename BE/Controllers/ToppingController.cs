using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToppingController : ControllerBase
    {
        private readonly IToppingService _service;

        public ToppingController(IToppingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var topping = await _service.GetByIdAsync(id);
            if (topping == null) return NotFound();
            return Ok(topping);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Topping topping)
        {
            await _service.CreateAsync(topping);
            return CreatedAtAction(nameof(GetById), new { id = topping.Id }, topping);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Topping topping)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            topping.Id = id;
            await _service.UpdateAsync(id, topping);
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
