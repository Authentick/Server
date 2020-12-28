using System;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.ReverseProxy.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Services.ReverseProxy.Authentication
{
    public class AuthenticationManager
    {
        private readonly AuthDbContext _authDbContext;
        private readonly IDataProtector _gatekeeperProxySsoSessionProtector;
        public const string AUTH_COOKIE = "gatekeeper.proxy.auth.cookie";

        public AuthenticationManager(
            AuthDbContext authDbContext,
            IDataProtectionProvider dataProtectionProvider
            )
        {
            _authDbContext = authDbContext;
            _gatekeeperProxySsoSessionProtector = dataProtectionProvider.CreateProtector("GATEKEEPER_PROXY_SSO");
        }


        public string GetTokenForId(Guid id) {
            return _gatekeeperProxySsoSessionProtector.Protect(id.ToString());
        }

        public bool IsAuthenticated(HttpContext context, out Guid? sessionId)
        {
            string? authCookie = context.Request.Cookies[AUTH_COOKIE];

            if (authCookie == null)
            {
                sessionId = null;
                return false;
            }

            try
            {
                string decryptedBlob = _gatekeeperProxySsoSessionProtector.Unprotect(authCookie);
                sessionId = new Guid(decryptedBlob);
                return true;
            }
            catch
            {
                sessionId = null;
                return false;
            }
        }

        public async Task<bool> IsAuthorizedAsync(Guid sessionId, MemorySingletonProxyConfigProvider.Route route)
        {
            ProxyAppSettings proxyAppSetting = await _authDbContext.ProxyAppSettings
                .AsNoTracking()
                .Where(p => p.AuthApp.UserGroups.Any(u => u.Members.Any(m => m.Sessions.Any(s => s.Id == sessionId && s.ExpiredTime == null))))
                .SingleOrDefaultAsync(p => p.Id == route.ProxySettingId);

            if (proxyAppSetting == null)
            {
                return false;
            }

            return true;
        }
    }
}
