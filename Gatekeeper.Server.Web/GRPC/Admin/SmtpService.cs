using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Shared.Admin;
using Gatekeeper.Server.Web.Services.Email;
using Gatekeeper.Server.Web.Services.Email.Credentials;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Server.GRPC.Admin
{
    [Authorize(Policy = "SuperAdministrator")]
    public class SmtpService : AuthServer.Shared.Admin.Smtp.SmtpBase
    {
        private readonly SmtpConnectivityChecker _smtpConnectivityChecker;
        private readonly SmtpCredentialManager _smtpCredentialManager;

        public SmtpService(
            SmtpConnectivityChecker smtpConnectivityChecker,
            SmtpCredentialManager smtpCredentialManager)
        {
            _smtpConnectivityChecker = smtpConnectivityChecker;
            _smtpCredentialManager = smtpCredentialManager;
        }

        public override async Task<ChangeSmtpSettingsReply> ChangeSmtpSettings(SmtpSettingsMessage request, ServerCallContext context)
        {
            SmtpCredentials credentials = new SmtpCredentials
            {
                Hostname = request.Hostname,
                Password = request.Password,
                Port = request.Port,
                SenderAddress = request.SenderAddress,
                Username = request.Username
            };
            await _smtpCredentialManager.StoreCredentialsAsync(credentials);

            return new ChangeSmtpSettingsReply
            {
                Success = true,
            };
        }

        public override async Task<SmtpSettingsMessage> GetSmtpSettings(Empty request, ServerCallContext context)
        {
            SmtpCredentials? credentials = await _smtpCredentialManager.FetchCredentialsAsync();
            if (credentials != null)
            {
                return new SmtpSettingsMessage
                {
                    Hostname = credentials.Hostname,
                    Password = credentials.Password,
                    Port = credentials.Port,
                    SenderAddress = credentials.SenderAddress,
                    Username = credentials.Username,
                };
            }

            return new SmtpSettingsMessage();
        }

        public override async Task<ValidateSmtpSettingsReply> ValidateSmtpSettings(SmtpSettingsMessage request, ServerCallContext context)
        {
            SmtpCredentials credentials = new SmtpCredentials
            {
                Hostname = request.Hostname,
                Password = request.Password,
                Port = request.Port,
                SenderAddress = request.SenderAddress,
                Username = request.Username,
            };

            return new ValidateSmtpSettingsReply
            {
                Success = await _smtpConnectivityChecker.CheckConnectivityAsnc(credentials),
            };
        }
    }
}
