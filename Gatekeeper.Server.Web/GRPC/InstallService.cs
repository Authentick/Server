using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto;
using AuthServer.Server.Services.TLS;
using AuthServer.Server.Services.TLS.BackgroundJob;
using AuthServer.Server.Services.User;
using AuthServer.Shared;
using Gatekeeper.Server.Services.FileStorage;
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
        private const string AUTH_KEY = "installer.auth_key";
        public const string INSTALLED_KEY = "installer.is_installed";
        public const string PRIMARY_DOMAIN_KEY = "installer.domain";
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
               .Where(s => s.Name == AUTH_KEY)
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
                Name = INSTALLED_KEY,
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

            SystemSetting? primaryDomainSetting = await _authDbContext.SystemSettings
                .SingleOrDefaultAsync(s => s.Name == PRIMARY_DOMAIN_KEY);
            if (primaryDomainSetting == null)
            {
                primaryDomainSetting = new SystemSetting
                {
                    Name = PRIMARY_DOMAIN_KEY,
                    Value = context.GetHttpContext().Request.Host.Host,
                };
                SystemSetting tlsCertificateSetting = new SystemSetting
                {
                    Name = "tls.acme.support",
                    Value = "false"
                };
                _authDbContext.AddRange(primaryDomainSetting, tlsCertificateSetting);
            }

            string snapFolder = PathProvider.GetApplicationDataFolder();
            string primaryDomainConfigFile = snapFolder + "/primary-domain.txt";
            await File.WriteAllTextAsync(primaryDomainConfigFile, primaryDomainSetting.Value);

            if (!CertificateRepository.TryGetCertificate(primaryDomainSetting.Value, out _))
            {
                ECDsa ecdsa = ECDsa.Create();
                CertificateRequest req = new CertificateRequest("cn=" + primaryDomainSetting.Value, ecdsa, HashAlgorithmName.SHA256);
                X509Certificate2 cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(15));

                CertificateRepository repository = new CertificateRepository();
                repository.StoreCertificate(primaryDomainSetting.Value, cert.Export(X509ContentType.Pfx));
            }

            _authDbContext.AddRange(installSetting, smtpHostnameSetting, smtpUsernameSetting, smtpPasswordSetting, smtpSenderAddress);
            await _authDbContext.SaveChangesAsync();

            return new SetupInstanceReply
            {
                Succeeded = true,
            };
        }

        private async Task<string> SetNewAuthToken()
        {
            SystemSetting? authKeySetting = await _authDbContext.SystemSettings
                .SingleOrDefaultAsync(s => s.Name == AUTH_KEY);
            string newAuthKey = _secureRandom.GetRandomString(16);

            if (authKeySetting == null)
            {
                authKeySetting = new SystemSetting
                {
                    Name = AUTH_KEY,
                };
                _authDbContext.Add(authKeySetting);
            }
            authKeySetting.Value = newAuthKey;

            await _authDbContext.SaveChangesAsync();
            return newAuthKey;
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

            string newAuthKey = await SetNewAuthToken();

            return new StartSetupReply
            {
                Success = true,
                AuthToken = newAuthKey,
            };
        }

        private async Task<bool> IsAccessible(string authToken)
        {
            bool isInstalled = await IsAlreadyInstalled();
            string existingAuthKey = await GetSetupAuthKey() ?? "";
            bool authKeysMatch = CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(existingAuthKey), Encoding.ASCII.GetBytes(authToken));
            if (!authKeysMatch || isInstalled)
            {
                return false;
            }

            return true;
        }

        public override async Task<Empty> IssueTlsCertificate(IssueTlsCertificateRequest request, ServerCallContext context)
        {
            if (!await IsAccessible(request.AuthToken))
            {
                return new Empty { };
            }

            SystemSetting tlsCertificateSetting = new SystemSetting
            {
                Name = "tls.acme.support",
                Value = "true",
            };
            _authDbContext.Add(tlsCertificateSetting);
            await _authDbContext.SaveChangesAsync();

            BackgroundJob.Enqueue<IRequestAcmeCertificateJob>(job => job.Request(request.ContactEmail, request.Domain));

            return new Empty();
        }

        public override async Task<IsTlsCertificateSetupReply> IsTlsCertificateSetup(IsTlsCertificateSetupRequest request, ServerCallContext context)
        {
            if (!await IsAccessible(request.AuthToken))
            {
                return new IsTlsCertificateSetupReply
                {
                    Success = false
                };
            }

            return new IsTlsCertificateSetupReply
            {
                Success = CertificateRepository.TryGetCertificate(request.Domain, out _),
            };
        }

        public override async Task<ChangeInstallTokenReply> ChangeInstallToken(ChangeInstallTokenRequest request, ServerCallContext context)
        {
            if (!await IsAccessible(request.AuthToken))
            {
                return new ChangeInstallTokenReply
                {
                    AuthToken = ""
                };
            }

            return new ChangeInstallTokenReply
            {
                AuthToken = await SetNewAuthToken(),
            };
        }
    }
}
