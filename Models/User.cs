using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("CoffeeShopUsers")]
    public class User
    {
        [DynamoDBHashKey("UserId")]
        public string UserId { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Username { get; set; } = string.Empty;

        //  dành riêng cho Shipper (local auth)
        [DynamoDBProperty]
        public string? PasswordHash { get; set; }

        //  để khóa / mở tài khoản (chung cho mọi role)
        [DynamoDBProperty]
        public bool IsActive { get; set; } = true;

        [DynamoDBProperty]
        public string Role { get; set; } = "User";

        [DynamoDBProperty]
        public int RewardPoints { get; set; } = 0;

        [DynamoDBProperty]
        public int VoucherCount { get; set; } = 0;

        [DynamoDBProperty]
        public List<Voucher>? AvailableVouchers { get; set; } = new();

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ========== THÔNG TIN SHIPPER (Chỉ cần thiết) ==========
        [DynamoDBProperty]
        public string? FullName { get; set; }

        [DynamoDBProperty]
        public string? Email { get; set; }

        // ========== CÁC TRƯỜNG KHÔNG CÒN BẮT BUỘC ==========
        [DynamoDBProperty]
        public string? PhoneNumber { get; set; }

        [DynamoDBProperty]
        public string? Address { get; set; }

        [DynamoDBProperty]
        public string? VehicleType { get; set; } // Xe máy, Ô tô, Xe đạp

        [DynamoDBProperty]
        public string? LicensePlate { get; set; }

        // ========== TRẠNG THÁI ĐĂNG KÝ ==========
        // Trạng thái đơn đăng ký: Pending, Approved, Rejected
        [DynamoDBProperty]
        public string RegistrationStatus { get; set; } = "Approved";

        [DynamoDBProperty]
        public DateTime? ApprovedAt { get; set; }

        [DynamoDBProperty]
        public string? ApprovedBy { get; set; } // UserId của Admin duyệt
    }
}
