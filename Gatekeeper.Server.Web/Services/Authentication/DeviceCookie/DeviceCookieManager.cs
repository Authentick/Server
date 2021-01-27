using System;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Gatekeeper.Server.Web.Services.Authentication.DeviceCookie
{
    public class DeviceCookieManager
    {
        public const string DEVICE_COOKIE_STRING = "DeviceId";
        private readonly AuthDbContext _authDbContext;
        private IDataProtector _deviceCookieProtector;

        public DeviceCookieManager(
            AuthDbContext authDbContext,
            IDataProtectionProvider dataProtectionProvider)
        {
            _authDbContext = authDbContext;
            _deviceCookieProtector = dataProtectionProvider.CreateProtector("DeviceCookieProtector");
        }

        private Guid? DecryptCookie(EncryptedDeviceCookie encryptedDeviceCookie)
        {
            string value = _deviceCookieProtector.Unprotect(encryptedDeviceCookie.EncryptedValue);
            bool isGuid = Guid.TryParse(value, out var deviceGuid);
            if (isGuid)
            {
                return deviceGuid;
            }

            return null;
        }

        public async Task<AuthServer.Server.Models.DeviceCookie?> GetDeviceCookieAsync(EncryptedDeviceCookie encryptedDeviceCookie)
        {
            Guid? deviceId = DecryptCookie(encryptedDeviceCookie);
            if (deviceId != null)
            {
                AuthServer.Server.Models.DeviceCookie? deviceCookie = await _authDbContext.DeviceCookies
                    .SingleOrDefaultAsync(d => d.Id == deviceId);
                return deviceCookie;
            }

            return null;
        }

        public AuthServer.Server.Models.DeviceCookie BuildNewDeviceCookie()
        {
            return new AuthServer.Server.Models.DeviceCookie { };
        }

        public EncryptedDeviceCookie GetEncryptedDeviceCookie(AuthServer.Server.Models.DeviceCookie deviceCookie)
        {
            string value = _deviceCookieProtector.Protect(deviceCookie.Id.ToString());

            return new EncryptedDeviceCookie(value);
        }

        public async Task<bool> IsCookieTrustedForUser(EncryptedDeviceCookie encryptedDeviceCookie, AppUser user)
        {
            Guid? deviceId = DecryptCookie(encryptedDeviceCookie);
            if (deviceId != null)
            {
                int count = await _authDbContext.Users
                    .AsNoTracking()
                    .Where(
                        u => u == user
                    )
                    .Where(
                        u => u.Sessions.Any(
                            s => s.DeviceCookie.Id == deviceId
                        )
                    )
                    .CountAsync();

                return count == 1;
            }

            return false;
        }
    }
}