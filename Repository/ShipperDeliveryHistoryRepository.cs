using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Repository
{
    public class ShipperDeliveryHistoryRepository
    {
        private readonly DynamoDBContext _context;

        public ShipperDeliveryHistoryRepository(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task AddHistoryAsync(ShipperDeliveryHistory history)
        {
            await _context.SaveAsync(history);
        }

        public async Task<ShipperDeliveryHistory?> GetHistoryAsync(string historyId)
        {
            return await _context.LoadAsync<ShipperDeliveryHistory>(historyId);
        }

        public async Task<List<ShipperDeliveryHistory>> GetShipperHistoryAsync(string shipperId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("ShipperId", ScanOperator.Equal, shipperId)
            };
            var search = _context.ScanAsync<ShipperDeliveryHistory>(conditions);
            return await search.GetRemainingAsync();
        }

        public async Task<List<ShipperDeliveryHistory>> GetOrderHistoryAsync(string orderId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("OrderId", ScanOperator.Equal, orderId)
            };
            var search = _context.ScanAsync<ShipperDeliveryHistory>(conditions);
            return await search.GetRemainingAsync();
        }

        public async Task UpdateHistoryAsync(ShipperDeliveryHistory history)
        {
            await _context.SaveAsync(history);
        }
    }
}
