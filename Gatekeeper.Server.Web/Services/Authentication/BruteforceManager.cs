using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using Gatekeeper.Server.Web.Services.Alerts;
using Gatekeeper.Server.Web.Services.Alerts.Types;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.Services.Authentication
{
    public class BruteforceManager
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager _userManager;
        private readonly AlertManager _alertManager;

        public BruteforceManager(
            AuthDbContext authDbContext,
            UserManager userManager,
            AlertManager alertManager)
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
            _alertManager = alertManager;
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

            await IssueAlertIfIpIsNowBlocked(ipAddress);
            if (user != null)
            {
                await IssueAlertIfUserIsNowBlocked(user);
            }
        }

        private async Task IssueAlertIfIpIsNowBlocked(IPAddress ipAddress)
        {
            if (await IsIpBlockedAsync(ipAddress))
            {
                BruteforceIpAddressAlert alert = new BruteforceIpAddressAlert(ipAddress);
                await _alertManager.AddAlertAsync(alert);
            }
        }

        private async Task IssueAlertIfUserIsNowBlocked(AppUser user)
        {
            if (await IsUserBlockedAsync(user))
            {
                BruteforceUserAlert alert = new BruteforceUserAlert(user);
                await _alertManager.AddAlertAsync(alert);
            }
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

        public async Task<bool> IsUserBlockedAsync(AppUser user)
        {
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
