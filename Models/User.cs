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

        [DynamoDBProperty]
        public string Role { get; set; } = "User";

        [DynamoDBProperty]
        public int RewardPoints { get; set; } = 0;

        [DynamoDBProperty]
        public int VoucherCount { get; set; } = 0;

        [DynamoDBProperty]
        public List<Voucher>? AvailableVouchers { get; set; } = new();

        [DynamoDBProperty]
        public bool IsActive { get; set; } = true;

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
