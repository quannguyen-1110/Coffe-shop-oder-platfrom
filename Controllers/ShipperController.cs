using CoffeeShopAPI.Services;
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipperController : ControllerBase
    {
        private readonly ShipperAuthService _shipperAuth;
        private readonly IConfiguration _config;

        public ShipperController(ShipperAuthService shipperAuth, IConfiguration config)
        {
            _shipperAuth = shipperAuth;
            _config = config;
        }

        // 🟢 Admin tạo Shipper mới
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ShipperRegisterRequest req)
        {
            try
            {
                var admin = User.Identity?.Name ?? "admin";
                var shipper = await _shipperAuth.RegisterShipperAsync(admin, req.Email, req.Password);
                return Ok(new { message = "✅ Shipper created successfully", shipper });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // 🟢 Shipper đăng nhập local
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ShipperLoginRequest req)
        {
            var user = await _shipperAuth.LoginAsync(req.Email, req.Password);
            if (user == null)
                return Unauthorized(new { error = "Invalid credentials or account inactive" });

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:LocalKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, "Shipper")
                },
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new { user.Username, user.Role }
            });
        }

        public class ShipperRegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string? Password { get; set; }
        }

        public class ShipperLoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
