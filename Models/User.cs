using Amazon.DynamoDBv2.DataModel;
using System;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("CoffeeShopUsers")]  // Tên bảng DynamoDB của bạn
    public class User
    {
        [DynamoDBHashKey]  // Khóa chính
        public string UserId { get; set; }  // Cognito UserSub

        [DynamoDBProperty]
        public string Username { get; set; }

        [DynamoDBProperty]
        public string Role { get; set; }

        [DynamoDBProperty]
        public bool IsActive { get; set; } = true;  // Trạng thái tài khoản

        [DynamoDBProperty]
        public int RewardPoints { get; set; } = 0;

        [DynamoDBProperty]
        public int VoucherCount { get; set; } = 0;

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
