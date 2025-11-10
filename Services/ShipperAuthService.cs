using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Services;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace CoffeeShopAPI.Services
{
    public class ShipperAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public ShipperAuthService(UserRepository userRepository, EmailService emailService, IConfiguration config)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;
        }

        //  Sinh mật khẩu tạm
        private string GeneratePassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder();
            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[4];
            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                var rnd = BitConverter.ToUInt32(buffer, 0);
                sb.Append(chars[(int)(rnd % (uint)chars.Length)]);
            }
            return sb.ToString();
        }

        //  Tạo tài khoản Shipper (chỉ Admin mới được gọi)
        public async Task<User> RegisterShipperAsync(string adminUsername, string email, string? password)
        {
            var existing = await _userRepository.GetUserByUsernameAsync(email);
            if (existing != null)
                throw new InvalidOperationException("User with this email already exists.");

            var pass = string.IsNullOrEmpty(password) ? GeneratePassword() : password;
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(pass);

            var newUser = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = email,
                Role = "Shipper",
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(newUser);

            // gửi email
            var subject = "Tài khoản Shipper của bạn đã được tạo";
            var body = $"Tài khoản: {email}\nMật khẩu: {pass}\n\nVui lòng đổi mật khẩu sau khi đăng nhập.";

            await _emailService.SendEmailAsync(email, subject, body);

            return newUser;
        }

        //  Login local cho Shipper
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(email);
            if (user == null || user.Role != "Shipper" || !user.IsActive)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }
    }
}
