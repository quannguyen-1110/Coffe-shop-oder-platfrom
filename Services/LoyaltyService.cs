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

            await _userRepo.UpdateUserAsync(user);
        }

        // ✅ USER CLICK NHẬN VOUCHER THỦ CÔNG (New)
        public async Task<Voucher> ClaimVoucherAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            if (user.RewardPoints < 100)
                throw new Exception("Không đủ điểm để nhận voucher (cần 100 điểm)");

            // Trừ 100 điểm
            user.RewardPoints -= 100;

            // Tạo voucher với random discount value (5% - 15%)
            var random = new Random();
            var discountPercent = random.Next(5, 16); // 5% đến 15%
            var discountValue = discountPercent / 100.0m;

            var voucher = new Voucher
            {
                Code = GenerateVoucherCode(),
                DiscountValue = discountValue,
                ExpirationDate = DateTime.UtcNow.AddMonths(1), // ✅ Hạn 1 tháng
                IsUsed = false,
                IsActive = true
            };

            // Đảm bảo list tồn tại và thêm voucher
            user.AvailableVouchers ??= new List<Voucher>();
            user.AvailableVouchers.Add(voucher);

            await _userRepo.UpdateUserAsync(user);

            return voucher;
        }

        // ✅ Generate voucher code đẹp hơn
        private string GenerateVoucherCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Bỏ ký tự dễ nhầm
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ✅ Áp dụng voucher giảm giá (giữ nguyên)
        public async Task<decimal> ApplyVoucherAsync(string userId, string code, decimal total)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var voucher = user.AvailableVouchers?
 .FirstOrDefault(v => !v.IsUsed && v.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (voucher == null)
                throw new Exception("Voucher không tồn tại hoặc đã được sử dụng");

            // Kiểm tra expired TRƯỚC KHI đánh dấu đã dùng
            if (voucher.ExpirationDate < DateTime.UtcNow)
                throw new Exception("Voucher đã hết hạn");

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

        // ✅ Check xem user có thể nhận voucher không
        public async Task<bool> CanClaimVoucherAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            return user != null && user.RewardPoints >= 100;
        }
    }
}
