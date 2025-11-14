using Amazon.DynamoDBv2.DataModel;

namespace CoffeeShopAPI.Models
{
    [DynamoDBTable("ShipperProfiles")]
    public class ShipperProfile
    {
        [DynamoDBHashKey("ShipperId")]
        public string ShipperId { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string FullName { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Email { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Phone { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string VehicleType { get; set; } = string.Empty; // Motorbike, Car, Bicycle

        [DynamoDBProperty]
        public string VehiclePlate { get; set; } = string.Empty; // Biển số xe

        [DynamoDBProperty]
        public string IdCard { get; set; } = string.Empty; // CMND/CCCD

        [DynamoDBProperty]
        public string BankAccount { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string BankName { get; set; } = string.Empty;

        [DynamoDBProperty]
        public decimal TotalEarnings { get; set; } = 0;

        [DynamoDBProperty]
        public int TotalDeliveries { get; set; } = 0;

        [DynamoDBProperty]
        public double Rating { get; set; } = 5.0;

        [DynamoDBProperty]
        public int TotalRatings { get; set; } = 0;

        [DynamoDBProperty]
        public bool IsActive { get; set; } = true;

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty]
        public DateTime? LastActiveAt { get; set; }
    }
}
