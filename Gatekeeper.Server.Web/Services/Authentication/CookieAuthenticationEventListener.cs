using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.User;
using Gatekeeper.Server.Services.Authentication.BackgroundJob;
using Gatekeeper.Server.Web.Services.Authentication.DeviceCookie;
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
        private readonly DeviceCookieManager _deviceCookieManager;

        public CookieAuthenticationEventListener(
            AuthDbContext authDbContext,
            UserManager userManager,
            SessionManager sessionManager,
            DeviceCookieManager deviceCookieManager
        )
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
            _sessionManager = sessionManager;
            _deviceCookieManager = deviceCookieManager;
        }

        public override async Task SigningIn(CookieSigningInContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.Principal);

            StringValues userAgent;
            context.HttpContext.Request.Headers.TryGetValue("User-Agent", out userAgent);

            string? deviceId;
            context.HttpContext.Request.Cookies.TryGetValue(DeviceCookieManager.DEVICE_COOKIE_STRING, out deviceId);
            DeviceCookie deviceCookie;

            if (deviceId == null)
            {
                deviceCookie = _deviceCookieManager.BuildNewDeviceCookie();
                _authDbContext.Add(deviceCookie);
                EncryptedDeviceCookie encryptedDeviceCookie = _deviceCookieManager.GetEncryptedDeviceCookie(deviceCookie);

                context.Response.Cookies.Append(
                    DeviceCookieManager.DEVICE_COOKIE_STRING,
                    encryptedDeviceCookie.EncryptedValue,
                    new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        IsEssential = true,
                        Expires = new DateTimeOffset(2038, 1, 1, 0, 0, 0, TimeSpan.FromHours(0)),
                        HttpOnly = true,
                    }
                );
            }
            else
            {
                DeviceCookie? potentialDeviceCookie = await _deviceCookieManager.GetDeviceCookieAsync(
                    new EncryptedDeviceCookie(deviceId)
                );
                if (potentialDeviceCookie == null)
                {
                    throw new Exception("User has an invalid device cookie: " + deviceId);
                }

                deviceCookie = potentialDeviceCookie;
            }

            AuthSession session = new AuthSession
            {
                CreationTime = SystemClock.Instance.GetCurrentInstant(),
                User = user,
                UserAgent = userAgent,
                DeviceCookie = deviceCookie,
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