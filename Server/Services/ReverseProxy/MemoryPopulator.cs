using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Services.ReverseProxy
{
    class MemoryPopulator
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
               MemorySingletonProxyConfigProvider.Route  route = new MemorySingletonProxyConfigProvider.Route(
                   setting.InternalHostname, 
                   setting.PublicHostname);
                _proxyConfigProvider.AddRoute(route);
            }
        }
    }
}
