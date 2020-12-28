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
    class AuthenticationManager
    {
        private readonly AuthDbContext _authDbContext;
        private readonly IDataProtector _gatekeeperProxySsoSessionProtector;

        public AuthenticationManager(
            AuthDbContext authDbContext,
            IDataProtectionProvider dataProtectionProvider
            )
        {
            _authDbContext = authDbContext;
            _gatekeeperProxySsoSessionProtector = dataProtectionProvider.CreateProtector("GATEKEEPER_PROXY_SSO");
        }

        public bool IsAuthenticated(HttpContext contect)
        {
            return true;
        }

        public async Task<bool> IsAuthorized(Guid sessionId, MemorySingletonProxyConfigProvider.Route route)
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

        public Guid GetSessionId()
        {
            return new Guid();
        }
    }
}
