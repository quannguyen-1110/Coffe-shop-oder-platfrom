using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(AuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        // Đăng ký tài khoản mới
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var success = await _authService.RegisterAsync(dto.Username, dto.Password);
            if (!success)
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

            return Ok(new { message = "Đăng ký thành công" });
        }

        // Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _authService.LoginAsync(dto.Username, dto.Password);
            if (user == null)
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

            // ✅ Tạo JWT token
            var token = _jwtService.GenerateToken(user.Username);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token
            });
        }

        // Đăng xuất
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Với JWT thì logout chỉ là xóa token ở phía client
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}
