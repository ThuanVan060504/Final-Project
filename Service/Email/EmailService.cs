using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Final_Project.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Lấy cấu hình từ appsettings.json
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];

            // Kiểm tra giá trị config
            if (string.IsNullOrEmpty(smtpHost) ||
                string.IsNullOrEmpty(smtpUser) ||
                string.IsNullOrEmpty(smtpPass))
            {
                throw new Exception("SMTP configuration is missing. Check appsettings.json!");
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage()
            {
                From = new MailAddress(smtpUser, "Shop Nội Thất G3TD"), // tên hiển thị
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);


            await client.SendMailAsync(mailMessage);
        }

    }
}
