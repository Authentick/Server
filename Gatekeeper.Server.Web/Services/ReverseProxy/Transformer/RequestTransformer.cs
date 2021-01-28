using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AuthServer.Server.Services.ReverseProxy.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Gatekeeper.Server.Web.Services.ReverseProxy.Transformer
{
    public class RequestTransformer : HttpTransformer
    {
        private readonly MemorySingletonProxyConfigProvider.Route _route;

        public RequestTransformer(MemorySingletonProxyConfigProvider.Route route)
        {
            _route = route;
        }

        public override async Task TransformRequestAsync(
            HttpContext httpContext,
            HttpRequestMessage proxyRequest,
            string destinationPrefix)
        {
            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);

            proxyRequest.Headers.Host = null;
            proxyRequest.Headers.Remove("X-Forwarded-For");
            proxyRequest.Headers.Remove("X-Forwarded-Host");

            IPAddress? ipAddress = httpContext.Connection.RemoteIpAddress;
            if (ipAddress != null)
            {
                string ipAddressString = (ipAddress.IsIPv4MappedToIPv6) ? ipAddress.MapToIPv4().ToString() : ipAddress.ToString();
                proxyRequest.Headers.Add("X-Forwarded-For", ipAddressString);
            }

            proxyRequest.Headers.Add("X-Forwarded-Host", _route.PublicHostname);

            if (httpContext.Request.Cookies.TryGetValue(AuthServer.Server.Services.ReverseProxy.Authentication.AuthenticationManager.AUTH_COOKIE, out string? authCookieValue))
            {
                // FIXME: This is currently also sent as cookie. Remove this and only send it as header.
                proxyRequest.Headers.Add(
                    "X-Gatekeeper-Jwt-Assertion",
                    authCookieValue
                );
            }
        }
    }
}
