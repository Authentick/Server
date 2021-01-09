using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.User;
using Gatekeeper.Server.Services.Authentication.BackgroundJob;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using NodaTime;

namespace AuthServer.Server.Services.Authentication
{
    public class CookieAuthenticationEventListener : CookieAuthenticationEvents
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager _userManager;
        private readonly SessionManager _sessionManager;

        public CookieAuthenticationEventListener(
            AuthDbContext authDbContext,
            UserManager userManager,
            SessionManager sessionManager
        )
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
            _sessionManager = sessionManager;
        }

        public override async Task SigningIn(CookieSigningInContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.Principal);

            StringValues userAgent;
            context.HttpContext.Request.Headers.TryGetValue("User-Agent", out userAgent);

            AuthSession session = new AuthSession
            {
                CreationTime = SystemClock.Instance.GetCurrentInstant(),
                User = user,
                UserAgent = userAgent,
            };
            _authDbContext.AuthSessions.Add(session);

            AuthSessionIp? authSessionIp = null;
            if (context.HttpContext.Connection.RemoteIpAddress != null)
            {
                authSessionIp = new AuthSessionIp
                {
                    AuthSession = session,
                    IpAddress = context.HttpContext.Connection.RemoteIpAddress,
                };
                _authDbContext.AuthSessionIps.Add(authSessionIp);
            }

            await _authDbContext.SaveChangesAsync();

            BackgroundJob.Enqueue<ISessionDeviceInfoResolver>(s => s.ResolveForAuthSession(session.Id));
            if (authSessionIp != null)
            {
                BackgroundJob.Enqueue<ISessionLocationResolver>(s => s.ResolveForAuthSessionIp(authSessionIp.Id));
            }

            ClaimsIdentity identity = (ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new Claim("cookie_identifier", session.Id.ToString()));
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            Guid userId = new Guid(_userManager.GetUserId(context.Principal));
            Guid cookieId = _sessionManager.GetCurrentSessionId(context.Principal);

            AuthSession session = await _sessionManager.GetActiveSessionById(userId, cookieId);

            if (session == null)
            {
                context.RejectPrincipal();
            }
            else
            {
                if (context.HttpContext.Connection.RemoteIpAddress != null)
                {
                    AuthSessionIp? authSessionIp = await _authDbContext.AuthSessionIps
                        .Where(s => s.AuthSession == session)
                        .Where(s => s.IpAddress == context.HttpContext.Connection.RemoteIpAddress)
                        .SingleOrDefaultAsync();

                    if (authSessionIp == null)
                    {
                        authSessionIp = new AuthSessionIp
                        {
                            AuthSession = session,
                            IpAddress = context.HttpContext.Connection.RemoteIpAddress
                        };
                        _authDbContext.AuthSessionIps.Add(authSessionIp);
                        await _authDbContext.SaveChangesAsync();
                        BackgroundJob.Enqueue<ISessionLocationResolver>(s => s.ResolveForAuthSessionIp(authSessionIp.Id));
                    }
                }

                _sessionManager.MarkSessionLastUsedNow(session);
            }
        }

    }
}