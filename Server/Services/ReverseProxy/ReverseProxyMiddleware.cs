using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AuthServer.Server.Services.TLS;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.ReverseProxy.Service.Proxy;
using Microsoft.ReverseProxy.Service.RuntimeModel.Transforms;

namespace AuthServer.Server.Services.ReverseProxy
{
    class ReverseProxyMiddleware
    {
        private readonly HttpMessageInvoker _httpClient;
        private readonly RequestDelegate _nextMiddleware;
        private readonly MemorySingletonProxyConfigProvider _proxyConfigProvider;
        private readonly IHttpProxy _httpProxy;
        private readonly AcmeChallengeSingleton _acmeChallengeSingleton;

        public ReverseProxyMiddleware(
            RequestDelegate nextMiddleware,
            MemorySingletonProxyConfigProvider proxyConfigProvider,
            IHttpProxy httpProxy,
            ProxyHttpClientProvider clientProvider,
            AcmeChallengeSingleton acmeChallengeSingleton)
        {
            _nextMiddleware = nextMiddleware;
            _proxyConfigProvider = proxyConfigProvider;
            _httpProxy = httpProxy;
            _httpClient = clientProvider.GetClient();
            _acmeChallengeSingleton = acmeChallengeSingleton;
        }

        public async Task Invoke(HttpContext context)
        {
            MemorySingletonProxyConfigProvider.Route? route = GetMatchingRoute(context);

            if (route != null)
            {
                bool shouldHandle = true;

                PathString requestPath = context.Request.Path;
                if (requestPath.StartsWithSegments("/.well-known/acme-challenge"))
                {
                    string challenge = ((string)requestPath).Split('/').Last();
                    if (_acmeChallengeSingleton.Challenges.ContainsKey(challenge))
                    {
                        shouldHandle = false;
                    }
                }

                if (shouldHandle)
                {
                    RequestProxyOptions proxyOptions = new RequestProxyOptions()
                    {
                        RequestTimeout = TimeSpan.FromSeconds(100),
                        Transforms = new Transforms(
                            copyRequestHeaders: true,
                            requestTransforms: Array.Empty<RequestParametersTransform>(),
                            requestHeaderTransforms: new Dictionary<string, RequestHeaderTransform>()
                            {
                            {
                                HeaderNames.Host,
                                new RequestHeaderValueTransform(string.Empty, append: false)
                            }
                            },
                            responseHeaderTransforms: new Dictionary<string, ResponseHeaderTransform>(),
                            responseTrailerTransforms: new Dictionary<string, ResponseHeaderTransform>()
                        )
                    };

                    await _httpProxy.ProxyAsync(context, route.InternalHostname, _httpClient, proxyOptions);
                    return;
                }
            }

            await _nextMiddleware(context);
        }

        private MemorySingletonProxyConfigProvider.Route? GetMatchingRoute(HttpContext context)
        {
            return _proxyConfigProvider.GetRoutes().SingleOrDefault(p => p.PublicHostname == context.Request.Host.Host);
        }
    }
}
