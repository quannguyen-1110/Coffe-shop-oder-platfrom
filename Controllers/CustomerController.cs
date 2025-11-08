using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] //  chỉ Admin mới được quản lý khách
    public class CustomerController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public CustomerController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //  1. Lấy danh sách khách hàng
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _userRepository.GetUsersByRoleAsync("User");
            return Ok(customers);
        }

        //  2. Lấy thông tin chi tiết 1 khách hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(string id)
        {
            var customer = await _userRepository.GetUserByIdAsync(id);
            if (customer == null || customer.Role != "User")
                return NotFound("User not found");

            return Ok(customer);
        }

        //  3. Khóa / Mở khóa tài khoản khách
        [HttpPut("{userId}/status")]
        public async Task<IActionResult> UpdateCustomerStatus(string userId, [FromBody] StatusRequest req)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found");
            if (user.Role != "User") return BadRequest("User is not a regular user");

            await _userRepository.UpdateUserStatusAsync(userId, req.IsActive);
            return Ok(new { message = req.IsActive ? "User activated" : "User deactivated" });
        }

        //  4. Xóa tài khoản khách hàng (Admin)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var customer = await _userRepository.GetUserByIdAsync(id);
            if (customer == null || customer.Role != "User")
                return NotFound("User not found");

            await _userRepository.UpdateUserStatusAsync(id, false); // chỉ tạm ngưng
            return Ok(new { message = "User account disabled (soft delete)" });
        }

        public class StatusRequest
        {
            public bool IsActive { get; set; }
        }
    }
}
