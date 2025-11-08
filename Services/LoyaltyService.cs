using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Services
{
    public class LoyaltyService
    {
        private readonly UserRepository _userRepo;

        public LoyaltyService(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // ✅ Thêm điểm thưởng cho user khi hoàn tất đơn hàng
        public async Task AddPointsAsync(string userId, decimal orderTotal)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // 1 điểm cho mỗi 10.000 VNĐ
            int earnedPoints = (int)(orderTotal / 10000);
            user.RewardPoints += earnedPoints;

            // Nếu đủ 100 điểm → tặng 1 voucher giảm 10%
            if (user.RewardPoints >= 100)
            {
                user.RewardPoints -= 100;

                // Đảm bảo list tồn tại
                user.AvailableVouchers ??= new List<Voucher>();
                user.AvailableVouchers.Add(new Voucher
                {
                    Code = Guid.NewGuid().ToString().Substring(0, 8),
                    DiscountValue = 0.1m,
                    ExpirationDate = DateTime.UtcNow.AddMonths(1),
                    IsUsed = false
                });
            }

            await _userRepo.UpdateUserAsync(user);
        }

        // ✅ Áp dụng voucher giảm giá
        public async Task<decimal> ApplyVoucherAsync(string userId, string code, decimal total)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var voucher = user.AvailableVouchers?
                .FirstOrDefault(v => !v.IsUsed && v.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (voucher == null)
                throw new Exception("Voucher not found or already used");

            // Kiểm tra expired TRƯỚC KHI đánh dấu đã dùng
            if (voucher.ExpirationDate < DateTime.UtcNow)
                throw new Exception("Voucher expired");

            // Đánh dấu voucher đã dùng
            voucher.IsUsed = true;
            await _userRepo.UpdateUserAsync(user);

            var discount = total * voucher.DiscountValue;
            return total - discount;
        }

        // ✅ Lấy danh sách voucher hiện tại
        public async Task<List<Voucher>> GetVouchersAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            return user.AvailableVouchers ?? new List<Voucher>();
        }
    }
}
