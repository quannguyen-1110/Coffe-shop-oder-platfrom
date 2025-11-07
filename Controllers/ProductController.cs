using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductRepository _productRepository;

        public ProductController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        //  Lấy danh sách tất cả sản phẩm
        [HttpGet]
        [AllowAnonymous] // ai cũng xem được
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return Ok(products);
        }

        //  Lấy chi tiết sản phẩm
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound("Product not found");
            return Ok(product);
        }

        //  Thêm sản phẩm (Admin, Staff)
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            await _productRepository.AddProductAsync(product);
            return Ok(new { message = " Product created successfully", product });
        }

        //  Cập nhật sản phẩm
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(string id, [FromBody] Product updated)
        {
            var existing = await _productRepository.GetProductByIdAsync(id);
            if (existing == null) return NotFound("Product not found");

            existing.Name = updated.Name;
            existing.Price = updated.Price;
            existing.Description = updated.Description;
            existing.IsAvailable = updated.IsAvailable;

            await _productRepository.UpdateProductAsync(existing);
            return Ok(new { message = " Product updated successfully", product = existing });
        }

        //  Xóa sản phẩm
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(string id)
        {
            await _productRepository.DeleteProductAsync(id);
            return Ok(new { message = " Product deleted successfully" });
        }
    }
}
