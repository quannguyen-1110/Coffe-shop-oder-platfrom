using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Services
{
    public class ShipperAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;

        public ShipperAuthService(UserRepository userRepository, AuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập cho Shipper (không dùng Cognito)
        /// </summary>
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            // Tìm user theo username
            var user = await _userRepository.GetUserByUsernameAsync(username.ToLower());

            if (user == null)
                throw new Exception("Invalid username or password");

            // Kiểm tra role
            if (user.Role != "Shipper")
                throw new Exception("This account is not a shipper account");

            // Kiểm tra trạng thái đăng ký
            if (user.RegistrationStatus != "Approved")
                throw new Exception("Your account is pending approval or has been rejected");

            // Kiểm tra tài khoản có bị khóa không
            if (!user.IsActive)
                throw new Exception("Your account has been locked. Please contact admin");

            // Kiểm tra password
            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new Exception("Password not set. Please contact admin");

            if (!_authService.VerifyPassword(password, user.PasswordHash))
                throw new Exception("Invalid username or password");

            // Generate JWT token
            var token = _authService.GenerateJwtToken(user.UserId, user.Username, user.Role);

            return new LoginResult
            {
                Token = token,
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName ?? user.Username,
                Email = user.Email ?? string.Empty,
                Role = user.Role
            };
        }

        /// <summary>
        /// Đổi mật khẩu cho Shipper
        /// </summary>
        public async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            if (user.Role != "Shipper")
                throw new Exception("Only shippers can use this endpoint");

            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new Exception("Password not set");

            // Verify old password
            if (!_authService.VerifyPassword(oldPassword, user.PasswordHash))
                throw new Exception("Old password is incorrect");

            // Validate new password
            if (newPassword.Length < 6)
                throw new Exception("New password must be at least 6 characters");

            // Hash and save new password
            user.PasswordHash = _authService.HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);
        }

        public class LoginResult
        {
            public string Token { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
