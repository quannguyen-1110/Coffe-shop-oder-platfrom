using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoyaltyController : ControllerBase
    {
        private readonly LoyaltyService _loyaltyService;
        private readonly UserRepository _userRepo;
        public LoyaltyController(LoyaltyService loyaltyService, UserRepository userRepo)
        {
            _loyaltyService = loyaltyService;
            _userRepo = userRepo;
        }

        //  1. Xem voucher của user hiện tại
        [Authorize(Roles = "User")]
        [HttpGet("my-vouchers")]
        public async Task<IActionResult> GetMyVouchers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue("sub");
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify user");

            var vouchers = await _loyaltyService.GetVouchersAsync(userId);
            return Ok(vouchers);
        }

        //  2. Xem điểm thưởng hiện tại
        [Authorize(Roles = "User")]
        [HttpGet("my-points")]
        public async Task<IActionResult> GetMyPoints()
        {
            
        
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue("sub");
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify user");
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");
            var vouchers = await _loyaltyService.GetVouchersAsync(userId);
            return Ok(new
            {
                userId,
                currentPoints = user.RewardPoints,
                availableVouchers = vouchers.Count(v => !v.IsUsed && v.ExpirationDate > DateTime.UtcNow),
                usedVouchers = vouchers.Count(v => v.IsUsed),
                expiredVouchers = vouchers.Count(v => !v.IsUsed && v.ExpirationDate <= DateTime.UtcNow)
            });
        }
            

        }

}
