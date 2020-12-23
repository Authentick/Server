using System.Collections.Generic;

namespace AuthServer.Server.Services.ReverseProxy
{
    internal class MemorySingletonProxyConfigProvider
    {
        private List<Route> Routes = new List<Route>();

        internal List<Route> GetRoutes() 
        {
            return Routes;
        }

        internal void AddRoute(Route route) 
        {
            Routes.Add(route);
        }

        internal class Route
        {
            public readonly string InternalHostname;
            public readonly string PublicHostname;

            public Route(string internalHostName, string publicHostName)
            {
                InternalHostname = internalHostName;
                PublicHostname = publicHostName;
            }
        }
    }
}
