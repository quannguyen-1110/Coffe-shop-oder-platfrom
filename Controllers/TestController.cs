using Microsoft.AspNetCore.Mvc;
using CoffeeShopAPI.Services;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly EmailService _emailService;

        public TestController(EmailService emailService)
        {
  _emailService = emailService;
        }

        /// <summary>
        /// Test g?i email qua AWS SES (dùng email ?ã verify trong SES Sandbox)
        /// </summary>
        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest req)
      {
            try
{
     await _emailService.SendShipperApprovalEmailAsync(
      req.ToEmail,
           "Test User",
        "testuser",
       "TempPass123!"
  );

                return Ok(new 
                { 
                  message = "? Email sent successfully!", 
   to = req.ToEmail,
          note = "Check your inbox (and spam folder)"
  });
 }
  catch (Exception ex)
            {
     return BadRequest(new 
   { 
 error = ex.Message,
   tip = "Make sure the email is verified in AWS SES Console (Sandbox mode)"
           });
         }
        }

        /// <summary>
        /// Test g?i email t? ch?i
        /// </summary>
        [HttpPost("send-rejection-email")]
   public async Task<IActionResult> SendRejectionEmail([FromBody] TestEmailRequest req)
     {
       try
            {
     await _emailService.SendShipperRejectionEmailAsync(
 req.ToEmail,
"Test User",
    "Thông tin ch?a ??y ??, vui lòng ??ng ký l?i"
            );

      return Ok(new 
       { 
     message = "? Rejection email sent!", 
         to = req.ToEmail 
      });
            }
            catch (Exception ex)
            {
          return BadRequest(new { error = ex.Message });
      }
      }

        public class TestEmailRequest
        {
    public string ToEmail { get; set; } = string.Empty;
        }
    }
}
