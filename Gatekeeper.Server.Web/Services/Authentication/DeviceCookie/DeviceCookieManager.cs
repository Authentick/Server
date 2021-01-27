using System;
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

        public async Task<AuthServer.Server.Models.DeviceCookie?> GetDeviceCookieAsync(EncryptedDeviceCookie encryptedDeviceCookie)
        {
            string value = _deviceCookieProtector.Unprotect(encryptedDeviceCookie.EncryptedValue);
            bool isGuid = Guid.TryParse(value, out var deviceGuid);
            if (isGuid)
            {
                AuthServer.Server.Models.DeviceCookie? deviceCookie = await _authDbContext.DeviceCookies
                    .SingleOrDefaultAsync(d => d.Id == deviceGuid);

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
    }
}