using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.Services.Authentication
{
    public class BruteforceManager
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager _userManager;

        public BruteforceManager(
            AuthDbContext authDbContext,
            UserManager userManager)
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
        }

        public async Task MarkInvalidLoginAttemptAsync(IPAddress ipAddress, string userAgent, string userName)
        {
            AppUser? user = await _userManager.FindByNameAsync(userName);

            InvalidLoginAttempt attempt = new InvalidLoginAttempt
            {
                UserName = userName,
                TargetUser = user,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                AttemptTime = SystemClock.Instance.GetCurrentInstant(),
            };
            _authDbContext.Add(attempt);
            await _authDbContext.SaveChangesAsync();
        }

        public async Task MarkInvalidTwoFactorAttemptAsync(IPAddress ipAddress, string userAgent, AppUser targetUser)
        {
            InvalidTwoFactorAttempt attempt = new InvalidTwoFactorAttempt
            {
                TargetUser = targetUser,
                UserAgent = userAgent,
                IPAddress = ipAddress,
                AttemptTime = SystemClock.Instance.GetCurrentInstant(),
            };
            _authDbContext.Add(attempt);
            await _authDbContext.SaveChangesAsync();
        }

        // FIXME: This currently does not consider IPv6. Current logic allows 10 login attempts in the last 10 minutes.
        public async Task<bool> IsIpBlockedAsync(IPAddress ipAddress)
        {
            Duration duration = Duration.FromMinutes(10);
            Instant currentTime = SystemClock.Instance.GetCurrentInstant();

            int attemptCount = await _authDbContext.InvalidLoginAttempts
                .AsNoTracking()
                .Where(l => l.IpAddress == ipAddress)
                .Where(l => currentTime.Minus(duration) <= l.AttemptTime)
                .CountAsync();

            return attemptCount >= 10;
        }

        public async Task<bool> IsUserBlockedAsync(AppUser user) {
            Duration duration = Duration.FromMinutes(10);
            Instant currentTime = SystemClock.Instance.GetCurrentInstant();

            int attemptCount = await _authDbContext.InvalidLoginAttempts
                .AsNoTracking()
                .Where(l => l.TargetUser == user)
                .Where(l => currentTime.Minus(duration) <= l.AttemptTime)
                .CountAsync();

            return attemptCount >= 10;
        }
    }
}
