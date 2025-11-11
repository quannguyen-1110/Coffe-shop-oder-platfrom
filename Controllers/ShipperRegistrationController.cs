using Microsoft.AspNetCore.Mvc;
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipperRegistrationController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public ShipperRegistrationController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

   /// <summary>
/// Guest ??ng ký làm Shipper (ch? c?n H? tên và Email)
     /// Email chính là username ?? login
        /// </summary>
    [HttpPost("register")]
        public async Task<IActionResult> RegisterShipper([FromBody] ShipperRegistrationRequest req)
        {
          // Validate
            if (string.IsNullOrWhiteSpace(req.FullName))
       return BadRequest(new { error = "Full name is required" });

        if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { error = "Email is required" });

            // Validate email format
        if (!IsValidEmail(req.Email))
 return BadRequest(new { error = "Invalid email format" });

    // ? Email chính là username luôn
          var username = req.Email.ToLower();

 // Ki?m tra email ?ã ??ng ký ch?a
            var existingUser = await _userRepository.GetUserByUsernameAsync(username);
          if (existingUser != null)
            {
         return Conflict(new { error = "Email này ?ã ???c ??ng ký. Vui lòng s? d?ng email khác." });
            }

            // T?o user m?i v?i tr?ng thái Pending
     var newUser = new User
        {
          UserId = Guid.NewGuid().ToString(),
 Username = username, // ? Username = Email
       Email = req.Email,
       FullName = req.FullName,
       Role = "Shipper",
      RegistrationStatus = "Pending", // ? Ch? duy?t
     IsActive = false, // Ch?a kích ho?t
          CreatedAt = DateTime.UtcNow
   };

        await _userRepository.AddUserAsync(newUser);

    return Ok(new
            {
        message = "??ng ký thành công! Vui lòng ch? admin phê duy?t.",
                userId = newUser.UserId,
           email = newUser.Email,
       note = "B?n s? nh?n email v?i thông tin ??ng nh?p sau khi ???c admin duy?t. S? d?ng email này làm tên ??ng nh?p."
      });
        }

private bool IsValidEmail(string email)
        {
    try
    {
       var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
        }
  catch
   {
                return false;
            }
        }

    public class ShipperRegistrationRequest
        {
  public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
  }
    }
}
