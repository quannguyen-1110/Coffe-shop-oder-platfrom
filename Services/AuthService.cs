using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

    public AuthService(IConfiguration config)
    {
        _provider = new AmazonCognitoIdentityProviderClient(RegionEndpoint.APSoutheast1);
        _clientId = config["Cognito:ClientId"];
        _userPoolId = config["Cognito:UserPoolId"];
    }

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

    /// <summary>
    /// Admin tạo tài khoản Shipper
    /// </summary>
    public async Task<SignUpResponse> CreateShipperAsync(
        string adminUsername,
        string shipperUsername,
        string password,
        UserRepository userRepository)
    {
        //  Kiểm tra role admin trong DynamoDB
        var admin = await userRepository.GetUserByUsernameAsync(adminUsername);
        if (admin == null || admin.Role != "Admin")
            throw new UnauthorizedAccessException(" Only admin can create shipper accounts.");

        //  Đăng ký shipper trong Cognito
        var request = new SignUpRequest
        {
            ClientId = _clientId,
            Username = shipperUsername,
            Password = password,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "custom:role", Value = "Shipper" }
            }
        };

        try
        {
            var response = await _provider.SignUpAsync(request);
            Console.WriteLine($"Shipper '{shipperUsername}' created by admin '{adminUsername}'");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateShipper error: {ex.Message}");
            throw;
        }
    }
}

}
