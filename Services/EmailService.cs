using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.Runtime;

namespace CoffeeShopAPI.Services
{
    public class EmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly string _fromEmail;
        private readonly string _replyToEmail;

        public EmailService(IConfiguration config)
        {
            var region = config["AWS:Region"] ?? "ap-southeast-1";
            var accessKey = config["AWS:AccessKey"];
            var secretKey = config["AWS:SecretKey"];

            // 🔐 Chọn cách xác thực AWS
            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                _sesClient = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.GetBySystemName(region));
                Console.WriteLine("✅ EmailService: Using explicit AWS credentials from config");
            }
            else
            {
                _sesClient = new AmazonSimpleEmailServiceClient(RegionEndpoint.GetBySystemName(region));
                Console.WriteLine("✅ EmailService: Using AWS CLI or environment credentials");
            }

            _fromEmail = config["AWS:SES:FromEmail"] ?? throw new Exception("Missing AWS:SES:FromEmail in appsettings.json");
            _replyToEmail = config["AWS:SES:ReplyToEmail"] ?? "support@yourdomain.com";
        }

        #region Public Methods

        /// <summary>
        /// Gửi email khi đơn đăng ký Shipper được duyệt.
        /// </summary>
        public async Task SendShipperApprovalEmailAsync(string toEmail, string fullName, string username, string temporaryPassword)
        {
            var subject = "🎉 Chúc mừng! Đơn đăng ký Shipper của bạn đã được duyệt";

            string htmlBody = GetEmailTemplate(
                title: "☕ Coffee Shop",
                headerColor: "#4CAF50",
                content: $@"
                    <h2>Xin chào {fullName},</h2>
                    <p>Chúc mừng! Đơn đăng ký làm Shipper của bạn đã được phê duyệt.</p>

                    <div style='background-color:#e8f5e9;padding:15px;border-left:4px solid #4CAF50;border-radius:4px;margin:20px 0;'>
                        <h3>🔑 Thông tin đăng nhập:</h3>
                        <p><strong>Email/Tên đăng nhập:</strong> {username}</p>
                        <p><strong>Mật khẩu tạm thời:</strong> 
                            <code style='background:#fff;padding:2px 8px;border-radius:3px;color:#d32f2f;'>{temporaryPassword}</code>
                        </p>
                    </div>

                    <p><strong>⚠️ Lưu ý quan trọng:</strong></p>
                    <ul>
                        <li>Sử dụng <strong>email của bạn</strong> làm tên đăng nhập</li>
                        <li>Vui lòng đổi mật khẩu ngay sau lần đăng nhập đầu tiên</li>
                        <li>Không chia sẻ thông tin đăng nhập với bất kỳ ai</li>
                        <li>Nếu quên mật khẩu, vui lòng liên hệ admin để được hỗ trợ</li>
                    </ul>

                    <p style='margin-top:30px;'>Chúc bạn làm việc hiệu quả!</p>
                "
            );

            string textBody = $@"
Xin chào {fullName},

Chúc mừng! Đơn đăng ký làm Shipper của bạn đã được phê duyệt.

THÔNG TIN ĐĂNG NHẬP:
- Email/Tên đăng nhập: {username}
- Mật khẩu tạm thời: {temporaryPassword}

⚠️ Lưu ý:
- Đổi mật khẩu ngay sau khi đăng nhập lần đầu.
- Không chia sẻ thông tin đăng nhập với bất kỳ ai.

Trân trọng,
Coffee Shop Team
";

            await SendEmailInternalAsync(toEmail, subject, htmlBody, textBody);
        }

        /// <summary>
        /// Gửi email khi đơn đăng ký Shipper bị từ chối.
        /// </summary>
        public async Task SendShipperRejectionEmailAsync(string toEmail, string fullName, string reason)
        {
            var subject = "Thông báo về đơn đăng ký Shipper";

            string htmlBody = GetEmailTemplate(
                title: "☕ Coffee Shop",
                headerColor: "#ff9800",
                content: $@"
                    <h2>Xin chào {fullName},</h2>
                    <p>Cảm ơn bạn đã quan tâm đến vị trí Shipper tại Coffee Shop.</p>
                    <p>Rất tiếc, đơn đăng ký của bạn chưa được chấp nhận lúc này.</p>

                    <div style='background-color:#fff3e0;padding:15px;border-left:4px solid #ff9800;border-radius:4px;margin:20px 0;'>
                        <p><strong>Lý do:</strong> {reason}</p>
                    </div>

                    <p>Bạn có thể đăng ký lại sau hoặc liên hệ với chúng tôi để biết thêm chi tiết.</p>
                    <p style='margin-top:30px;'>Trân trọng,<br/><strong>Coffee Shop Team</strong></p>
                "
            );

            string textBody = $@"
Xin chào {fullName},

Cảm ơn bạn đã quan tâm đến vị trí Shipper tại Coffee Shop.
Rất tiếc, đơn đăng ký của bạn chưa được chấp nhận.

Lý do: {reason}

Bạn có thể đăng ký lại sau hoặc liên hệ với chúng tôi để biết thêm chi tiết.

Trân trọng,
Coffee Shop Team
";

            await SendEmailInternalAsync(toEmail, subject, htmlBody, textBody);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Mẫu HTML email thống nhất (header + footer)
        /// </summary>
        private string GetEmailTemplate(string title, string headerColor, string content)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f9f9f9;
            padding: 0;
            margin: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background-color: {headerColor};
            color: white;
            text-align: center;
            padding: 20px;
        }}
        .content {{
            padding: 25px 30px;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #777;
            padding: 15px;
            background-color: #f5f5f5;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{title}</h1>
        </div>
        <div class='content'>
            {content}
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>Liên hệ hỗ trợ: {_replyToEmail}</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Gửi email qua AWS SES
        /// </summary>
        private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody, string textBody)
        {
            var request = new SendEmailRequest
            {
                Source = _fromEmail,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { toEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content { Data = htmlBody, Charset = "UTF-8" },
                        Text = new Content { Data = textBody, Charset = "UTF-8" }
                    }
                },
                ReplyToAddresses = new List<string> { _replyToEmail }
            };

            try
            {
                await _sesClient.SendEmailAsync(request);
                Console.WriteLine($"✅ Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
