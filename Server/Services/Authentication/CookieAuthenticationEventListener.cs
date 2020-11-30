using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.User;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            Guid userId = new Guid(_userManager.GetUserId(context.Principal));
            Guid cookieId = _sessionManager.GetCurrentSessionId(context.Principal);

            AuthSession session = await _sessionManager.GetActiveSessionById(userId, cookieId);

            if (session == null)
            {
                context.RejectPrincipal();
            }
            else
            {
                _sessionManager.MarkSessionLastUsedNow(session);
            }
        }

    }
}