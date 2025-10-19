using BE.Data;
using BE.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace BE.Services
{
    public class AuthService
    {
        private readonly MongoDbService _mongoService;
        private readonly JwtService _jwtService;
        private readonly IMongoCollection<User> _users;

        public AuthService(MongoDbService mongoService, JwtService jwtService)
        {
            _mongoService = mongoService;
            _jwtService = jwtService;
            _users = _mongoService.GetCollection<User>("Users");
        }

        // Đăng ký
        public async Task<bool> RegisterAsync(string username, string password)
        {
            var existingUser = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (existingUser != null)
                return false; // user đã tồn tại

            var hashedPassword = HashPassword(password);
            var newUser = new User
            {
                Username = username,
                Password = hashedPassword
            };

            await _users.InsertOneAsync(newUser);
            return true;
        }

        // Đăng nhập
        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null) return null;

            if (!VerifyPassword(password, user.Password))
                return null;

            // ✅ Đúng: chỉ trả về User để Controller tạo JWT
            return user;
        }

        // Hash mật khẩu
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Kiểm tra mật khẩu
        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
