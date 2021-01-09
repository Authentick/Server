using System;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Gatekeeper.Server.Services.DeviceDetection;
using Microsoft.EntityFrameworkCore;

namespace Gatekeeper.Server.Services.Authentication.BackgroundJob
{
    public class SessionDeviceInfoResolver : ISessionDeviceInfoResolver
    {
        private readonly AuthDbContext _authDbContext;
        private readonly DeviceDetectionManager _deviceDetectionManager;

        public SessionDeviceInfoResolver(
            AuthDbContext authDbContext,
            DeviceDetectionManager deviceDetectionManager)
        {
            _authDbContext = authDbContext;
            _deviceDetectionManager = deviceDetectionManager;
        }

        public async Task ResolveForAuthSession(Guid authSessionId)
        {
            AuthSession session = await _authDbContext.AuthSessions
                .SingleAsync(s => s.Id == authSessionId);

            if (session.UserAgent != null)
            {
                DeviceInfo? info = await _deviceDetectionManager.TryResolveLocationAsync(session.UserAgent);
                if (info != null)
                {
                    DeviceInformation.DeviceTypeEnum type = DeviceInformation.DeviceTypeEnum.Unknown;
                    if (info.IsSmartphone)
                    {
                        type = DeviceInformation.DeviceTypeEnum.Smartphone;
                    }
                    else if (info.IsTablet)
                    {
                        type = DeviceInformation.DeviceTypeEnum.Tablet;
                    }
                    else if (info.IsDesktop)
                    {
                        type = DeviceInformation.DeviceTypeEnum.Desktop;
                    }

                    session.DeviceInfo = new DeviceInformation
                    {
                        Brand = info.BrandName,
                        Browser = info.BrowserName,
                        DeviceType = type,
                        Model = info.ModelName,
                        OperatingSystem = info.OperatingSystemName,
                    };
                    await _authDbContext.SaveChangesAsync();
                }
            }
        }
    }
}
