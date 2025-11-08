using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Repository
{
    public class OrderRepository
    {
        private readonly DynamoDBContext _context;

        public OrderRepository(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task AddOrderAsync(Order order)
        {
            await _context.SaveAsync(order);
        }

        public async Task<Order?> GetOrderByIdAsync(string id)
        {
            return await _context.LoadAsync<Order>(id);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await _context.SaveAsync(order);
        }

        public async Task<List<Order>> GetOrdersByUserAsync(string userId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("UserId", ScanOperator.Equal, userId)
            };
            return await _context.ScanAsync<Order>(conditions).GetRemainingAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.ScanAsync<Order>(new List<ScanCondition>()).GetRemainingAsync();
        }
    }
}
