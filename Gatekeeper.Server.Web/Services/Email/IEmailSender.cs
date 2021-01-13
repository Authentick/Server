using System.Threading.Tasks;

namespace AuthServer.Server.Services.Email
{
    interface IEmailSender
    {
        public Task SendEmailAsync(
            string email,
            string recipientName,
            string subject,
            string message);
    }
}
