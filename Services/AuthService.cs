using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Services
{
    public class AuthService
    {
        private readonly IAmazonCognitoIdentityProvider _provider;
        private readonly string _clientId;
        private readonly string _userPoolId;
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
_config = config;
        _provider = new AmazonCognitoIdentityProviderClient(RegionEndpoint.APSoutheast1);
      _clientId = config["Cognito:ClientId"];
            _userPoolId = config["Cognito:UserPoolId"];
        }

        // ========== COGNITO AUTH (Customer & Admin) ==========

        /// <summary>
   /// Đăng ký người dùng mới vào AWS Cognito
        /// </summary>
      public async Task<SignUpResponse> RegisterAsync(string username, string password, string role)
        {
    var request = new SignUpRequest
  {
    ClientId = _clientId,
         Username = username,
           Password = password,
       UserAttributes = new List<AttributeType>
          {
           new AttributeType
            {
              Name = "custom:role",
    Value = role
      }
        }
            };

            try
         {
          var response = await _provider.SignUpAsync(request);
   Console.WriteLine($"User '{username}' registered successfully!");
           return response;
 }
            catch (Exception ex)
       {
          Console.WriteLine($"Register error: {ex.Message}");
        throw;
            }
        }

        /// <summary>
     /// Đăng nhập người dùng (lấy JWT token)
        /// </summary>
        public async Task<InitiateAuthResponse> LoginAsync(string username, string password)
  {
        var request = new InitiateAuthRequest
        {
      AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
         ClientId = _clientId,
    AuthParameters = new Dictionary<string, string>
          {
      { "USERNAME", username },
      { "PASSWORD", password }
         }
   };

     try
          {
         var response = await _provider.InitiateAuthAsync(request);
  Console.WriteLine($"User '{username}' logged in successfully!");
            return response;
  }
    catch (Exception ex)
   {
      Console.WriteLine($"Login error: {ex.Message}");
              throw;
            }
        }

        /// <summary>
        /// Đăng xuất người dùng (hủy token)
        /// </summary>
        public async Task GlobalSignOutAsync(string accessToken)
        {
            var request = new GlobalSignOutRequest
        {
     AccessToken = accessToken
  };

       try
            {
           await _provider.GlobalSignOutAsync(request);
       Console.WriteLine("User logged out successfully!");
            }
     catch (Exception ex)
         {
           Console.WriteLine($"Logout error: {ex.Message}");
         throw;
    }
        }

        public async Task<ConfirmSignUpResponse> ConfirmSignUpAsync(string username, string confirmationCode)
        {
   var request = new ConfirmSignUpRequest
    {
   ClientId = _clientId,
  Username = username,
        ConfirmationCode = confirmationCode
      };

            try
      {
    var response = await _provider.ConfirmSignUpAsync(request);
  return response;
        }
  catch (Exception ex)
 {
      Console.WriteLine($"ConfirmSignUp error: {ex.Message}");
throw;
  }
        }

        public async Task<ResendConfirmationCodeResponse> ResendConfirmationCodeAsync(string username)
        {
            var request = new ResendConfirmationCodeRequest
 {
                ClientId = _clientId,
   Username = username
            };

            try
            {
                var response = await _provider.ResendConfirmationCodeAsync(request);
         return response;
   }
         catch (Exception ex)
        {
 Console.WriteLine($"ResendConfirmationCode error: {ex.Message}");
     throw;
 }
      }

        // ========== LOCAL AUTH (Shipper Only) ==========

        /// <summary>
        /// Hash password sử dụng PBKDF2
        /// </summary>
        public string HashPassword(string password)
 {
       byte[] salt = RandomNumberGenerator.GetBytes(16);
      var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            byte[] hashBytes = new byte[48];
       Array.Copy(salt, 0, hashBytes, 0, 16);
       Array.Copy(hash, 0, hashBytes, 16, 32);

 return Convert.ToBase64String(hashBytes);
     }

        /// <summary>
        /// Verify password với hash đã lưu
        /// </summary>
        public bool VerifyPassword(string password, string storedHash)
        {
     byte[] hashBytes = Convert.FromBase64String(storedHash);
    byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
      byte[] hash = pbkdf2.GetBytes(32);

    for (int i = 0; i < 32; i++)
  {
       if (hashBytes[i + 16] != hash[i])
         return false;
      }
       return true;
        }

        /// <summary>
        /// Generate JWT token cho Shipper (local auth)
        /// </summary>
        public string GenerateJwtToken(string userId, string username, string role)
{
      var secretKey = _config["Jwt:LocalKey"] ?? throw new Exception("Missing Jwt:LocalKey");
         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

   var claims = new[]
{
        new Claim(JwtRegisteredClaimNames.Sub, userId),
     new Claim(JwtRegisteredClaimNames.UniqueName, username),
  new Claim(ClaimTypes.Role, role),
         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
     audience: _config["Jwt:Audience"],
       claims: claims,
  expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
                signingCredentials: creds
            );

   return new JwtSecurityTokenHandler().WriteToken(token);
  }

        /// <summary>
        /// Generate random password (8 ký tự)
        /// </summary>
   public string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
       var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ========== ADMIN OPERATIONS (Cognito) ==========

        //  ADMIN: Khóa user trong Cognito
        public async Task AdminDisableUserAsync(string username)
      {
       var request = new AdminDisableUserRequest
     {
    UserPoolId = _userPoolId,
     Username = username
      };
      await _provider.AdminDisableUserAsync(request);
        }

        //  ADMIN: Mở khóa user trong Cognito
        public async Task AdminEnableUserAsync(string username)
        {
var request = new AdminEnableUserRequest
            {
  UserPoolId = _userPoolId,
          Username = username
    };
       await _provider.AdminEnableUserAsync(request);
        }

        //  ADMIN: Reset mật khẩu (Cognito sẽ gửi email)
     public async Task AdminResetUserPasswordAsync(string username)
        {
 var request = new AdminResetUserPasswordRequest
         {
    UserPoolId = _userPoolId,
      Username = username
            };
    await _provider.AdminResetUserPasswordAsync(request);
    }
    }
}
