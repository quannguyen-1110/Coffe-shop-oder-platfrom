using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CakeController : ControllerBase
    {
        private readonly ICakeService _service;

        public CakeController(ICakeService service)
        {
            _service = service;
        }

        // Lấy danh sách bánh
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cakes = await _service.GetAllAsync();
            return Ok(cakes);
        }

        // Lấy chi tiết bánh theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var cake = await _service.GetByIdAsync(id);
            if (cake == null) return NotFound();
            return Ok(cake);
        }

        // Thêm bánh mới
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Cake cake)
        {
            await _service.CreateAsync(cake);
            return CreatedAtAction(nameof(GetById), new { id = cake.Id }, cake);
        }

        // Cập nhật bánh
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Cake cake)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            cake.Id = id;
            await _service.UpdateAsync(id, cake);
            return NoContent();
        }

        // Xóa bánh
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }

        // Giảm số lượng tồn kho
        [HttpPatch("{id}/decreaseStock/{quantity}")]
        public async Task<IActionResult> DecreaseStock(string id, int quantity)
        {
            var success = await _service.DecreaseStockAsync(id, quantity);
            if (!success) return BadRequest("Not enough stock");
            return NoContent();
        }
    }
}
