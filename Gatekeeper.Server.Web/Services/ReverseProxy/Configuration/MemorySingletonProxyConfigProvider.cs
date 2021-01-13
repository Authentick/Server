using System;
using System.Collections.Generic;

namespace AuthServer.Server.Services.ReverseProxy.Configuration
{
    public class MemorySingletonProxyConfigProvider
    {
        private Dictionary<string, Route> Routes = new Dictionary<string, Route>();

        internal Dictionary<string, Route> GetRoutes()
        {
            return Routes;
        }

        internal void AddRoute(Route route)
        {
            Routes.TryAdd(route.PublicHostname, route);
        }

        public class Route
        {
            public readonly string InternalHostname;
            public readonly string PublicHostname;
            public readonly Guid ProxySettingId;
            public readonly HashSet<string> PublicRoutes;

            public Route(
                Guid proxySettingId, 
                string internalHostName, 
                string publicHostName,
                HashSet<string> publicRoutes)
            {
                ProxySettingId = proxySettingId;
                InternalHostname = internalHostName;
                PublicHostname = publicHostName;
                PublicRoutes = publicRoutes;
            }
        }
    }
}
