using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Services.ReverseProxy.Configuration
{
    public class MemoryPopulator
    {
        private readonly AuthDbContext _authDbContext;
        private readonly MemorySingletonProxyConfigProvider _proxyConfigProvider;

        public MemoryPopulator(
            AuthDbContext authDbContext,
            MemorySingletonProxyConfigProvider singletonProxyConfigProvider
        )
        {
            _authDbContext = authDbContext;
            _proxyConfigProvider = singletonProxyConfigProvider;
        }

        public async Task PopulateFromDatabase()
        {
            List<ProxyAppSettings> proxySettings = await _authDbContext.ProxyAppSettings.ToListAsync();

            foreach (ProxyAppSettings setting in proxySettings)
            {
                HashSet<string> publicRoutes;
                if (setting.EndpointsWithoutAuth == null)
                {
                    publicRoutes = new HashSet<string>();
                }
                else
                {
                    publicRoutes = new HashSet<string>(setting.EndpointsWithoutAuth);
                }

                MemorySingletonProxyConfigProvider.Route route = new MemorySingletonProxyConfigProvider.Route(
                    setting.Id,
                    setting.InternalHostname,
                    setting.PublicHostname,
                    publicRoutes
                    );
                _proxyConfigProvider.AddRoute(route);
            }
        }
    }
}
