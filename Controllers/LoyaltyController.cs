using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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

    /// <summary>
     /// Xem voucher của user hiện tại
    /// </summary>
        [Authorize(Roles = "User")]
    [HttpGet("my-vouchers")]
        public async Task<IActionResult> GetMyVouchers()
        {
            try
    {
  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
          ?? User.FindFirstValue("sub");
            
      if (string.IsNullOrEmpty(userId))
         return Unauthorized("Cannot identify user");

  var vouchers = await _loyaltyService.GetVouchersAsync(userId);
       
    return Ok(new
        {
   userId,
  totalVouchers = vouchers.Count,
            availableVouchers = vouchers.Where(v => !v.IsUsed && v.ExpirationDate > DateTime.UtcNow).ToList(),
   usedVouchers = vouchers.Where(v => v.IsUsed).ToList(),
          expiredVouchers = vouchers.Where(v => !v.IsUsed && v.ExpirationDate <= DateTime.UtcNow).ToList()
 });
    }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
   }
        }

  /// <summary>
        /// Xem điểm thưởng hiện tại và thống kê voucher
  /// </summary>
 [Authorize(Roles = "User")]
        [HttpGet("my-points")]
        public async Task<IActionResult> GetMyPoints()
        {
    try
            {
       var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
       ?? User.FindFirstValue("sub");
    
  if (string.IsNullOrEmpty(userId))
           return Unauthorized("Cannot identify user");
          
   var user = await _userRepo.GetUserByIdAsync(userId);
   if (user == null)
           return NotFound("User not found");
           
          var vouchers = await _loyaltyService.GetVouchersAsync(userId);
        var canClaimVoucher = await _loyaltyService.CanClaimVoucherAsync(userId);

      return Ok(new
{
             userId,
        currentPoints = user.RewardPoints,
  pointsToNextVoucher = Math.Max(0, 100 - (user.RewardPoints % 100)),
            canClaimVoucher, // ✅ Cho FE biết có thể nhận voucher không
              statistics = new
                    {
      availableVouchers = vouchers.Count(v => !v.IsUsed && v.ExpirationDate > DateTime.UtcNow),
               usedVouchers = vouchers.Count(v => v.IsUsed),
         expiredVouchers = vouchers.Count(v => !v.IsUsed && v.ExpirationDate <= DateTime.UtcNow),
    totalVouchers = vouchers.Count
         }
      });
     }
     catch (Exception ex)
            {
         return BadRequest(new { error = ex.Message });
}
        }

        /// <summary>
 /// 🎁 USER CLICK NHẬN VOUCHER (NEW ENDPOINT)
        /// </summary>
    [Authorize(Roles = "User")]
        [HttpPost("claim-voucher")]
        public async Task<IActionResult> ClaimVoucher()
     {
      try
            {
     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
    ?? User.FindFirstValue("sub");
   
     if (string.IsNullOrEmpty(userId))
       return Unauthorized("Cannot identify user");

    var voucher = await _loyaltyService.ClaimVoucherAsync(userId);
          
     return Ok(new
      {
       success = true,
           message = $"🎉 Chúc mừng! Bạn nhận được voucher giảm {voucher.DiscountValue * 100:F0}%",

      voucher = new
 {
         code = voucher.Code,
      discountPercent = voucher.DiscountValue * 100,
  discountValue = voucher.DiscountValue,
    expirationDate = voucher.ExpirationDate,
           validUntil = voucher.ExpirationDate.ToString("dd/MM/yyyy HH:mm")
     },
         remainingPoints = (await _userRepo.GetUserByIdAsync(userId))?.RewardPoints ?? 0
        });
  }
     catch (Exception ex)
    {
  return BadRequest(new { 
        success = false, 
            error = ex.Message 
     });
  }
 }

    /// <summary>
     /// ✅ Validate voucher trước khi áp dụng (cho FE checkout)
        /// </summary>
        [Authorize(Roles = "User")]
        [HttpPost("validate-voucher")]
        public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherRequest request)
        {
   try
         {
          var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
          ?? User.FindFirstValue("sub");
   
                if (string.IsNullOrEmpty(userId))
      return Unauthorized("Cannot identify user");

    var user = await _userRepo.GetUserByIdAsync(userId);
     if (user == null)
return NotFound("User not found");

                var voucher = user.AvailableVouchers?
          .FirstOrDefault(v => !v.IsUsed && v.Code.Equals(request.VoucherCode, StringComparison.OrdinalIgnoreCase));

        if (voucher == null)
        {
     return Ok(new
                    {
               isValid = false,
            message = "Voucher không tồn tại hoặc đã được sử dụng"
      });
            }

      if (voucher.ExpirationDate < DateTime.UtcNow)
             {
  return Ok(new
             {
          isValid = false,
      message = "Voucher đã hết hạn"
             });
                }

       var discountAmount = request.OrderTotal * voucher.DiscountValue;
         var finalAmount = request.OrderTotal - discountAmount;

 return Ok(new
        {
        isValid = true,
         voucher = new
      {
         code = voucher.Code,
  discountPercent = voucher.DiscountValue * 100,
     expirationDate = voucher.ExpirationDate
            },
    calculation = new
                 {
         orderTotal = request.OrderTotal,
   discountAmount,
      finalAmount
      },
           message = $"Áp dụng voucher giảm {voucher.DiscountValue * 100:F0}%"
         });
         }
      catch (Exception ex)
       {
    return BadRequest(new { error = ex.Message });
}
  }

        public class ValidateVoucherRequest
        {
 public string VoucherCode { get; set; } = string.Empty;
            public decimal OrderTotal { get; set; }
        }
    }
}
