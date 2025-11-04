using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CoffeeShopAPI.Models;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Repository
{
    public class UserRepository
    {
        private readonly DynamoDBContext _context;

        public UserRepository(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.SaveAsync(user);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.LoadAsync<User>(userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _context.SaveAsync(user);
        }
    }
}
