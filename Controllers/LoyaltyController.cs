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
    public class LoyaltyController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly VoucherRepository _voucherRepository;

        public LoyaltyController(UserRepository userRepository, VoucherRepository voucherRepository)
        {
            _userRepository = userRepository;
            _voucherRepository = voucherRepository;
        }

        //  1. ADMIN tạo voucher mới
        [Authorize(Roles = "Admin")]
        [HttpPost("voucher")]
        public async Task<IActionResult> CreateVoucher([FromBody] Voucher voucher)
        {
            await _voucherRepository.AddVoucherAsync(voucher);
            return Ok(new { message = " Voucher created successfully!", voucher });
        }

        //  2. Xem danh sách tất cả voucher (ai cũng xem được)
        [AllowAnonymous]
        [HttpGet("voucher")]
        public async Task<IActionResult> GetAllVouchers()
        {
            var vouchers = await _voucherRepository.GetAllVouchersAsync();
            return Ok(vouchers);
        }

        //  3. Customer đổi điểm lấy voucher
        [Authorize(Roles = "Customer")]
        [HttpPost("redeem/{voucherId}")]
        public async Task<IActionResult> RedeemVoucher(string voucherId, [FromQuery] string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) return NotFound(" User not found.");

            var voucher = await _voucherRepository.GetVoucherByIdAsync(voucherId);
            if (voucher == null) return NotFound(" Voucher not found.");
            if (!voucher.IsActive) return BadRequest(" Voucher is inactive.");

            if (user.RewardPoints < voucher.RequiredPoints)
                return BadRequest(" Not enough points to redeem this voucher.");

            // Trừ điểm và tăng số voucher
            user.RewardPoints -= voucher.RequiredPoints;
            user.VoucherCount += 1;

            await _userRepository.UpdateUserAsync(user);

            return Ok(new
            {
                message = " Voucher redeemed successfully!",
                remainingPoints = user.RewardPoints,
                voucherCode = voucher.Code
            });
        }

        //  4. Admin xóa voucher
        [Authorize(Roles = "Admin")]
        [HttpDelete("voucher/{id}")]
        public async Task<IActionResult> DeleteVoucher(string id)
        {
            await _voucherRepository.DeleteVoucherAsync(id);
            return Ok(new { message = " Voucher deleted successfully!" });
        }
    }
}
