using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CoffeeShopAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromAddress;
        private readonly bool _useSsl;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _smtpHost = _config["Smtp:Host"];
            _smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
            _smtpUser = _config["Smtp:User"];
            _smtpPass = _config["Smtp:Pass"];
            _fromAddress = _config["Smtp:From"];
            _useSsl = bool.Parse(_config["Smtp:UseSsl"] ?? "true");
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass)
            };

            var mail = new MailMessage(_fromAddress, to, subject, body)
            {
                IsBodyHtml = false
            };

            await client.SendMailAsync(mail);
        }
    }
}
