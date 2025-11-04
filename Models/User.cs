using Amazon.DynamoDBv2.DataModel;
using System;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("CoffeeShopUsers")]  // Tên b?ng DynamoDB c?a b?n
    public class User
    {
        [DynamoDBHashKey]  // Khóa chính
        public string UserId { get; set; }  // L?u Cognito UserSub

        [DynamoDBProperty]
        public string Username { get; set; }

        [DynamoDBProperty]
        public string Role { get; set; }

        [DynamoDBProperty]
        public int RewardPoints { get; set; } = 0;  // ?i?m th??ng

        [DynamoDBProperty]
        public int VoucherCount { get; set; } = 0;  // S? voucher hi?n có

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
