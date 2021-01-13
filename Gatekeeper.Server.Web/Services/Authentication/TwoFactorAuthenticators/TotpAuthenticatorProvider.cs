using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.TwoFactorAuthenticators.Implementation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.Services.Authentication.TwoFactorAuthenticators
{
    class TotpAuthenticatorProvider : AuthenticatorTokenProvider<AppUser>
    {
        private readonly AuthDbContext _authDbContext;
        public static readonly string ProviderName = "Authenticator";

        public TotpAuthenticatorProvider(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public override async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<AppUser> manager, AppUser user)
        {
            int devicesCount = await _authDbContext.UserTotpDevices
                .AsNoTracking()
                .Where(u => u.User == user)
                .CountAsync();

            return devicesCount > 0;
        }

        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<AppUser> manager, AppUser user)
        {
            int code;

            if (!int.TryParse(token, out code))
            {
                return false;
            }

            List<UserTotpDevice> devices = await _authDbContext.UserTotpDevices
                .Where(u => u.User == user)
                .ToListAsync();

            var unixTimestamp = Convert.ToInt64(Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
            var timestep = Convert.ToInt64(unixTimestamp / 30);

            foreach (UserTotpDevice device in devices)
            {
                var hash = new HMACSHA1(Base32.FromBase32(device.SharedSecret));

                for (int i = -2; i <= 2; i++)
                {
                    var expectedCode = Rfc6238AuthenticationService.ComputeTotp(hash, (ulong)(timestep + i), modifier: null);
                    if (expectedCode == code)
                    {
                        device.LastUsedTime = SystemClock.Instance.GetCurrentInstant();
                        await _authDbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
