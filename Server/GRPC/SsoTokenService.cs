using System;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.ReverseProxy.Authentication;
using AuthServer.Shared;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.GRPC
{
    public class SsoTokenService : AuthServer.Shared.SsoTokenService.SsoTokenServiceBase
    {
        private readonly SessionManager _sessionManager;
        private readonly AuthenticationManager _authenticationManager;
        private readonly AuthDbContext _authDbContext;

        public SsoTokenService(
            SessionManager sessionManager,
            AuthDbContext authDbContext,
            AuthenticationManager authenticationManager)
        {
            _sessionManager = sessionManager;
            _authenticationManager = authenticationManager;
            _authDbContext = authDbContext;
        }

        public override async Task<SsoTokenReply> GetCurrentSessionToken(SsoTokenRequest request, ServerCallContext context)
        {
            Guid cookieId = _sessionManager.GetCurrentSessionId(context.GetHttpContext().User);
            ProxyAppSettings setting = await _authDbContext.ProxyAppSettings
                .SingleAsync(s => s.Id == new Guid(request.ProxyId));
            string ssoToken = _authenticationManager.GetTokenForId(cookieId);

            return new SsoTokenReply
            {
                RedirectUrl = "https://" + setting.PublicHostname + "/gatekeeper-proxy-sso?" + SingleSignOnHandler.AUTH_PARAM_NAME + "=" + ssoToken,
            };
        }
    }
}
