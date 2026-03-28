using System.Net;
using System.Net.Mail;

namespace WebCUOI.Helpers
{
    public class Common
    {
        public static void SendMail(string toEmail, string subject, string body)
        {
            string fromEmail = "yourappemail@gmail.com"; // thay bằng Gmail của bạn
            string password = "yourapppassword"; // app password tạo từ Google

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            smtpClient.Send(mailMessage);
        }
    }
}
