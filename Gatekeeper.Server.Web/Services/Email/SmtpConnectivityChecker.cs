using System.Threading.Tasks;
using Gatekeeper.Server.Web.Services.Email.Credentials;
using MailKit.Net.Smtp;

namespace Gatekeeper.Server.Web.Services.Email
{
    public class SmtpConnectivityChecker
    {
        public async Task<bool> CheckConnectivityAsnc(SmtpCredentials credentials)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(credentials.Hostname, credentials.Port);
                    await client.AuthenticateAsync(credentials.Username, credentials.Password);
                    await client.DisconnectAsync(true);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}