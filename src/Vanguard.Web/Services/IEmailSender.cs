using System.Threading.Tasks;

namespace Vanguard.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
