using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto.JWT;
using AuthServer.Server.Services.ReverseProxy.Configuration;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Services.ReverseProxy.Authentication
{
    public class AuthenticationManager
    {
        private readonly AuthDbContext _authDbContext;
        public const string AUTH_COOKIE = "gatekeeper.proxy.auth.cookie";
        private readonly JwtFactory _jwtFactory;

        public AuthenticationManager(
            AuthDbContext authDbContext,
            JwtFactory jwtFactory
            )
        {
            _authDbContext = authDbContext;
            _jwtFactory = jwtFactory;
        }


        public string GetToken(AppUser user, ProxyAppSettings setting, Guid sessionId)
        {
            string token = _jwtFactory.Build()
                .Subject(user.Id.ToString())
                .Id(sessionId)
                .Audience(setting.InternalHostname)
                .IssuedAt(DateTime.UtcNow)
                .Encode();

            return token;
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
                Dictionary<string, object> decodedToken = _jwtFactory.Build()
                    .MustVerifySignature()
                    .Decode<Dictionary<string, object>>(authCookie);

                sessionId = new Guid((string)decodedToken["jti"]);
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
                .Where(
                    p => p.AuthApp.UserGroups.Any(
                        u => u.Members.Any(
                            m => m.Sessions.Any(
                                s =>
                                    s.Id == sessionId &&
                                    s.ExpiredTime == null &&
                                    m.IsDisabled == false
                            )
                        )
                    )
                )
                .SingleOrDefaultAsync(p => p.Id == route.ProxySettingId);

            if (proxyAppSetting == null)
            {
                return false;
            }

            return true;
        }
    }
}
