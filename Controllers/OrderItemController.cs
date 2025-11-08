using CoffeeShopAPI.Models;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly OrderItemService _orderItemService;

        public OrderItemController(OrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        // ✅ Validate item trước khi thêm vào order (helper endpoint)
        [HttpPost("validate")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ValidateItem([FromBody] OrderItem item)
        {
            try
            {
                var validated = await _orderItemService.ValidateAndCalculateItemAsync(item);
                return Ok(new 
                { 
                    message = "Item is valid",
                    item = validated
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
