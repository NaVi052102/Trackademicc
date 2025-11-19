using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Trackademic.Core.Interfaces;

namespace Trackademic.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            string? server = emailSettings["Server"];
            string? portStr = emailSettings["Port"];
            string? senderEmail = emailSettings["SenderEmail"];
            string? senderName = emailSettings["SenderName"];
            string? password = emailSettings["Password"];
            string? enableSslStr = emailSettings["EnableSSL"];

            if (string.IsNullOrWhiteSpace(server) ||
                string.IsNullOrWhiteSpace(portStr) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(enableSslStr))
            {
                throw new InvalidOperationException("Email settings are not properly configured.");
            }

            int port = int.Parse(portStr);
            bool enableSsl = bool.Parse(enableSslStr);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            using (var smtpClient = new SmtpClient(server, port))
            {
                smtpClient.Credentials = new NetworkCredential(senderEmail, password);
                smtpClient.EnableSsl = enableSsl;
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}