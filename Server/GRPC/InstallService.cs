using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto;
using AuthServer.Server.Services.TLS;
using AuthServer.Server.Services.User;
using AuthServer.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC
{
    public class InstallService : AuthServer.Shared.Install.InstallBase
    {
        private readonly AuthDbContext _authDbContext;
        private readonly SecureRandom _secureRandom;
        private readonly string AUTH_KEY = "installer.auth_key";
        private readonly string INSTALLED_KEY = "installer.is_installed";
        private readonly UserManager _userManager;

        public InstallService(
            AuthDbContext authDbContext,
            SecureRandom secureRandom,
            UserManager userManager)
        {
            _authDbContext = authDbContext;
            _secureRandom = secureRandom;
            _userManager = userManager;
        }

        private async Task<bool> IsAlreadyInstalled()
        {
            SystemSetting? isInstalledSetting = await _authDbContext.SystemSettings
                .AsNoTracking()
                .Where(s => s.Name == INSTALLED_KEY && s.Value == "true")
                .SingleOrDefaultAsync();

            return isInstalledSetting != null;
        }

        private async Task<string?> GetSetupAuthKey()
        {
            SystemSetting? isInstalledSetting = await _authDbContext.SystemSettings
               .AsNoTracking()
               .Where(s => s.Name == this.AUTH_KEY)
               .SingleOrDefaultAsync();

            if (isInstalledSetting == null)
            {
                return null;
            }

            return isInstalledSetting.Value;
        }

        public override async Task<CheckIsInstalledReply> CheckIsInstalled(Empty request, ServerCallContext context)
        {
            return new CheckIsInstalledReply
            {
                IsInstalled = await IsAlreadyInstalled(),
            };
        }

        public override async Task<SetupInstanceReply> SetupInstance(SetupInstanceRequest request, ServerCallContext context)
        {
            bool isInstalled = await IsAlreadyInstalled();
            string existingAuthKey = await GetSetupAuthKey() ?? "";
            bool authKeysMatch = CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(existingAuthKey), Encoding.ASCII.GetBytes(request.AuthToken));

            if (isInstalled || existingAuthKey == "" || !authKeysMatch)
            {
                return new SetupInstanceReply
                {
                    ErrorMessage = "Installation failed for security reasons.",
                    Succeeded = false,
                };
            }

            AppUser user = new AppUser
            {
                EmailConfirmed = true,
                UserName = request.AccountData.Username,
                Email = request.AccountData.Email,
            };

            await _userManager.CreateAsync(user, request.AccountData.Password);
            await _userManager.AddToRoleAsync(user, "admin");

            SystemSetting installSetting = new SystemSetting
            {
                Name = this.INSTALLED_KEY,
                Value = "true",
            };
            SystemSetting smtpHostnameSetting = new SystemSetting
            {
                Name = "smtp.hostname",
                Value = request.SmtpSettings.Hostname,
            };
            SystemSetting smtpUsernameSetting = new SystemSetting
            {
                Name = "smtp.username",
                Value = request.SmtpSettings.Username,
            };
            SystemSetting smtpPasswordSetting = new SystemSetting
            {
                Name = "smtp.password",
                Value = request.SmtpSettings.Password,
            };
            SystemSetting smtpSenderAddress = new SystemSetting
            {
                Name = "smtp.senderAddress",
                Value = request.SmtpSettings.SenderAddress,
            };
            SystemSetting smtpPort = new SystemSetting
            {
                Name = "smtp.port",
                Value = request.SmtpSettings.Port.ToString(),
            };

            SystemSetting tlsCertificateSetting = new SystemSetting
            {
                Name = "tls.acme.support",
            };

            if (request.TlsData != null)
            {
                tlsCertificateSetting.Value = "true";
                BackgroundJob.Enqueue<IRequestAcmeCertificateJob>(job => job.Request(request.TlsData.ContactEmail, request.TlsData.Domain));
            }
            else
            {
                tlsCertificateSetting.Value = "false";
            }

            _authDbContext.AddRange(installSetting, smtpHostnameSetting, smtpUsernameSetting, smtpPasswordSetting, smtpSenderAddress, tlsCertificateSetting);
            await _authDbContext.SaveChangesAsync();

            return new SetupInstanceReply
            {
                Succeeded = true,
            };
        }

        public override async Task<StartSetupReply> StartSetup(Empty request, ServerCallContext context)
        {
            bool isInstalled = await IsAlreadyInstalled();
            string? existingAuthKey = await GetSetupAuthKey();

            if (isInstalled || existingAuthKey != null)
            {
                return new StartSetupReply
                {
                    Success = false,
                };
            }

            string newAuthKey = _secureRandom.GetRandomString(16);
            SystemSetting authKeySetting = new SystemSetting
            {
                Name = this.AUTH_KEY,
                Value = newAuthKey,
            };
            _authDbContext.Add(authKeySetting);
            await _authDbContext.SaveChangesAsync();

            return new StartSetupReply
            {
                Success = true,
                AuthToken = newAuthKey,
            };
        }
    }
}
