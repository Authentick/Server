using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace AuthServer.Server.Services.Authentication
{
    public class CookieAuthenticationEventListener : CookieAuthenticationEvents
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager<AppUser> _userManager;

        public CookieAuthenticationEventListener(
            AuthDbContext authDbContext,
            UserManager<AppUser> userManager
        )
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
        }

        public override async Task SignedIn(CookieSignedInContext context) {
            AppUser user = await _userManager.GetUserAsync(context.Principal);

            AuthSession session = new AuthSession {
                CreationTime = SystemClock.Instance.GetCurrentInstant(),
                Name = "TODO Name",
                User = user,
            };

            _authDbContext.AuthSessions.Add(session);
            await _authDbContext.SaveChangesAsync();
        }
    }
}