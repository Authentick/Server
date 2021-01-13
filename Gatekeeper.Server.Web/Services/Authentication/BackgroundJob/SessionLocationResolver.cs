using System;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Gatekeeper.Server.Services.GeoLocation;
using Microsoft.EntityFrameworkCore;

namespace Gatekeeper.Server.Services.Authentication.BackgroundJob
{
    public class SessionLocationResolver : ISessionLocationResolver
    {
        private readonly AuthDbContext _authDbContext;
        private readonly GeoLocationManager _geoLocationManager;

        public SessionLocationResolver(
            AuthDbContext authDbContext,
            GeoLocationManager geoLocationManager)
        {
            _authDbContext = authDbContext;
            _geoLocationManager = geoLocationManager;
        }

        public async Task ResolveForAuthSessionIp(Guid authSessionIpId)
        {
            AuthSessionIp sessionIp = await _authDbContext.AuthSessionIps
                .SingleAsync(s => s.Id == authSessionIpId);


            GeoLocation.GeoLocation? result = await _geoLocationManager.TryResolveLocationAsync(sessionIp.IpAddress);
            if (result != null)
            {
                sessionIp.City = result.City;
                sessionIp.Country = result.CountryCode;
                await _authDbContext.SaveChangesAsync();
            }
        }
    }
}
