using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;
using System.Collections.Generic;
using System.Linq;
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

        // Lấy user theo username
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("Username", ScanOperator.Equal, username)
            };

            var results = await _context.ScanAsync<User>(conditions).GetRemainingAsync();
            return results.FirstOrDefault();
        }

        //  Lấy danh sách user theo role 
        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("Role", ScanOperator.Equal, role)
            };

            var results = await _context.ScanAsync<User>(conditions).GetRemainingAsync();
            return results.ToList();
        }

        // ✅ Thêm method lấy tất cả users cho dashboard
        public async Task<List<User>> GetAllUsersAsync()
        {
            var results = await _context.ScanAsync<User>(new List<ScanCondition>()).GetRemainingAsync();
            return results.ToList();
        }

        //  Cập nhật trạng thái kích hoạt 
        public async Task UpdateUserStatusAsync(string userId, bool isActive)
        {
            var user = await _context.LoadAsync<User>(userId);
            if (user == null) return;

            user.IsActive = isActive;
            await _context.SaveAsync(user);
        }
    }
}
