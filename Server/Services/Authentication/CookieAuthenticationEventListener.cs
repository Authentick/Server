using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace AuthServer.Server.Services.Authentication
{
    public class CookieAuthenticationEventListener : CookieAuthenticationEvents
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly SessionManager _sessionManager;

        public CookieAuthenticationEventListener(
            AuthDbContext authDbContext,
            UserManager<AppUser> userManager,
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

            AuthSession session = new AuthSession
            {
                CreationTime = SystemClock.Instance.GetCurrentInstant(),
                Name = "TODO Name",
                User = user,
            };

            _authDbContext.AuthSessions.Add(session);
            await _authDbContext.SaveChangesAsync();

            ClaimsIdentity identity = (ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new Claim("cookie_identifier", session.Id.ToString()));
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.Principal);
            Guid cookieId = _sessionManager.GetCurrentSessionId(context.Principal);

            bool isActiveSession = _sessionManager.IsSessionActive(user, cookieId);

            if (!isActiveSession)
            {
                context.RejectPrincipal();
            } else {
                _sessionManager.MarkSessionLastUsedNow(cookieId);
            }
        }

    }
}