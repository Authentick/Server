using System;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.ReverseProxy.Authentication;
using AuthServer.Server.Services.User;
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
        private readonly UserManager _userManager;

        public SsoTokenService(
            SessionManager sessionManager,
            AuthDbContext authDbContext,
            AuthenticationManager authenticationManager,
            UserManager userManager)
        {
            _sessionManager = sessionManager;
            _authenticationManager = authenticationManager;
            _authDbContext = authDbContext;
            _userManager = userManager;
        }

        public override async Task<SsoTokenReply> GetCurrentSessionToken(SsoTokenRequest request, ServerCallContext context)
        {
            Guid cookieId = _sessionManager.GetCurrentSessionId(context.GetHttpContext().User);
            ProxyAppSettings setting = await _authDbContext.ProxyAppSettings
                .SingleAsync(s => s.Id == new Guid(request.ProxyId));
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            string ssoToken = _authenticationManager.GetToken(user, setting, cookieId);

            return new SsoTokenReply
            {
                TargetUrl = "https://" + setting.PublicHostname + "/gatekeeper-proxy-sso",
                SsoToken = ssoToken,
            };
        }
    }
}
