// Services/IEmailSender.cs
using System.Threading.Tasks;

namespace WebCUOI.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}