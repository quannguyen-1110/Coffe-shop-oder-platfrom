using Amazon.DynamoDBv2.DataModel;
using System;

namespace CoffeeShopAPI.Models
{
    public class Voucher
    {
        [DynamoDBProperty]
        public string Code { get; set; } = string.Empty;

        [DynamoDBProperty]
        public decimal DiscountValue { get; set; } = 0.1m;

        [DynamoDBProperty]
        public int RequiredPoints { get; set; } = 100; // cần bao nhiêu điểm để đổi

        [DynamoDBProperty]
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        [DynamoDBProperty]
        public bool IsUsed { get; set; } = false;

        [DynamoDBProperty]
        public bool IsActive { get; set; } = true;
    }
}
