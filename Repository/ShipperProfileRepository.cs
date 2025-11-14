using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Repository
{
    public class ShipperProfileRepository
    {
        private readonly DynamoDBContext _context;

        public ShipperProfileRepository(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task<ShipperProfile?> GetProfileAsync(string shipperId)
        {
            return await _context.LoadAsync<ShipperProfile>(shipperId);
        }

        public async Task CreateOrUpdateProfileAsync(ShipperProfile profile)
        {
            await _context.SaveAsync(profile);
        }

        public async Task<List<ShipperProfile>> GetAllProfilesAsync()
        {
            var search = _context.ScanAsync<ShipperProfile>(new List<ScanCondition>());
            return await search.GetRemainingAsync();
        }

        public async Task<List<ShipperProfile>> GetActiveShippersAsync()
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("IsActive", ScanOperator.Equal, true)
            };
            var search = _context.ScanAsync<ShipperProfile>(conditions);
            return await search.GetRemainingAsync();
        }
    }
}
