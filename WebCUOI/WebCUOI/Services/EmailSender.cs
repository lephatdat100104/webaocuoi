// Services/EmailSender.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // Để đọc cấu hình từ appsettings.json

namespace WebCUOI.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Lấy thông tin cấu hình từ appsettings.json
            // SỬA TÊN SECTION TỪ "MailSettings" THÀNH "EmailSettings" ĐỂ KHỚP VỚI appsettings.json
            var emailSettings = _configuration.GetSection("EmailSettings");

            var smtpHost = emailSettings["SmtpHost"];

            // SỬ DỤNG TRYPARSE ĐỂ TRÁNH LỖI ARGUMENTNULL EXCEPTION NẾU GIÁ TRỊ NULL HOẶC KHÔNG HỢP LỆ
            int smtpPort;
            if (!int.TryParse(emailSettings["SmtpPort"], out smtpPort))
            {
                // Ghi log hoặc xử lý lỗi nếu không parse được port, sử dụng giá trị mặc định
                // Ví dụ: log.LogError("SmtpPort is missing or invalid in appsettings.json.");
                smtpPort = 587; // Giá trị mặc định phổ biến cho SMTP
            }

            // SỬA TÊN KEYS TỪ "SmtpUser" THÀNH "SmtpUsername" VÀ "SmtpPass" THÀNH "SmtpPassword"
            // ĐỂ KHỚP VỚI appsettings.json
            var smtpUsername = emailSettings["SmtpUsername"]; // Địa chỉ email của bạn (ví dụ: your_email@gmail.com)
            var smtpPassword = emailSettings["SmtpPassword"]; // Mật khẩu ứng dụng của bạn (không phải mật khẩu Gmail)

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(smtpUsername)); // Người gửi - dùng smtpUsername
            emailMessage.To.Add(MailboxAddress.Parse(email));          // Người nhận
            emailMessage.Subject = subject;                             // Tiêu đề email
            emailMessage.Body = new TextPart(TextFormat.Html) { Text = message }; // Nội dung email (có thể là HTML)

            using var smtp = new SmtpClient();
            try
            {
                // Kiểm tra xem có cần EnableSsl không (nếu có trong appsettings)
                bool enableSsl;
                if (!bool.TryParse(emailSettings["EnableSsl"], out enableSsl))
                {
                    enableSsl = true; // Mặc định là true nếu không có hoặc không parse được
                }

                // Cấu hình SecureSocketOptions dựa trên EnableSsl
                var secureSocketOption = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

                await smtp.ConnectAsync(smtpHost, smtpPort, secureSocketOption);
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword); // Dùng smtpUsername và smtpPassword
                await smtp.SendAsync(emailMessage);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}