using System.Text.Json;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Gatekeeper.Server.Web.Services.Email.Credentials;
using Microsoft.EntityFrameworkCore;

namespace Gatekeeper.Server.Web.Services.Email.Credentials
{
    public class SmtpCredentialManager
    {
        private readonly AuthDbContext _authDbContext;
        private const string SETTINGS_KEY = "smtp.settings";

        public SmtpCredentialManager(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public async Task StoreCredentialsAsync(SmtpCredentials credentials)
        {
            string newValues = JsonSerializer.Serialize(credentials);

            SystemSetting? smtpSettings = await _authDbContext.SystemSettings
                .SingleOrDefaultAsync(s => s.Name == SETTINGS_KEY);

            if (smtpSettings == null)
            {
                smtpSettings = new SystemSetting
                {
                    Name = SETTINGS_KEY,
                };
                _authDbContext.Add(smtpSettings);
            }
            smtpSettings.Value = newValues;
            await _authDbContext.SaveChangesAsync();
        }

        public async Task<SmtpCredentials?> FetchCredentialsAsync()
        {
            SystemSetting? smtpSettings = await _authDbContext.SystemSettings
                .SingleOrDefaultAsync(s => s.Name == SETTINGS_KEY);
            if (smtpSettings != null)
            {
                SmtpCredentials? credentials = JsonSerializer.Deserialize<SmtpCredentials>(smtpSettings.Value);
                return credentials;
            }

            return null;
        }
    }
}
