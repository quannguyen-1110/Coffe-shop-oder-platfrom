using Amazon.DynamoDBv2.DataModel;
using System;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("CoffeeShopVouchers")]
    public class Voucher
    {
        [DynamoDBHashKey]
        public string VoucherId { get; set; } = Guid.NewGuid().ToString();

        [DynamoDBProperty]
        public string Code { get; set; } = string.Empty; // Mã voucher

        [DynamoDBProperty]
        public int RequiredPoints { get; set; } // Bao nhiêu điểm để đổi

        [DynamoDBProperty]
        public decimal DiscountValue { get; set; } // Giảm bao nhiêu tiền (% hoặc số tiền)

        [DynamoDBProperty]
        public bool IsActive { get; set; } = true;

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty]
        public DateTime? ExpiryDate { get; set; }
    }
}
