using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AuthServer.Server.Services.Crypto;
using AuthServer.Server.Services.ReverseProxy.Authentication;
using AuthServer.Server.Services.ReverseProxy.Configuration;
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

        public async Task Invoke(
            HttpContext context,
            AuthenticationManager authenticationManager,
            SingleSignOnHandler singleSignOnHandler,
            ConfigurationProvider configurationProvider,
            SecureRandom secureRandom
            )
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
                    configurationProvider.TryGet(AuthServer.Server.GRPC.InstallService.PRIMARY_DOMAIN_KEY, out string primaryDomain);

                    bool isAuthRequest = singleSignOnHandler.IsAuthRequest(context);
                    if (isAuthRequest)
                    {
                        singleSignOnHandler.Handle(context);
                        return;
                    }

                    bool isPublicEndpoint = route.PublicRoutes.Contains(context.Request.Path);

                    if (!isPublicEndpoint)
                    {
                        bool isAuthenticated = authenticationManager.IsAuthenticated(context, out Guid? sessionId);

                        if (!isAuthenticated)
                        {
                            string csrf = secureRandom.GetRandomString(16);
                            context.Response.Cookies.Append("gatekeeper.csrf", csrf);

                            Dictionary<string, string> queryDictionary = new Dictionary<string, string>()
                            {
                                {"id", route.ProxySettingId.ToString()},
                                {"csrf", csrf},
                            };

                            UriBuilder uriBuilder = new UriBuilder();
                            uriBuilder.Scheme = "https";
                            uriBuilder.Host = primaryDomain;
                            uriBuilder.Path = "/auth/sso-connect";
                            uriBuilder.Query = await ((new System.Net.Http.FormUrlEncodedContent(queryDictionary)).ReadAsStringAsync());

                            context.Response.Redirect(uriBuilder.ToString(), false);
                            return;
                        }
                        else
                        {
                            if (sessionId == null)
                            {
                                // This should never happen
                                return;
                            }

                            bool isAuthorized = await authenticationManager.IsAuthorizedAsync((Guid)sessionId, route);
                            if (!isAuthorized)
                            {
                                context.Response.Redirect("https://" + primaryDomain + "/auth/403");
                                return;
                            }
                        }
                    }

                    Dictionary<string, RequestHeaderTransform> requestHeaderTransforms = new Dictionary<string, RequestHeaderTransform>()
                    {
                        {
                            "X-Forwarded-For",
                            new RequestHeaderValueTransform(context.Connection.RemoteIpAddress.ToString(), append: false)
                        },
                        {
                            HeaderNames.Host,
                            new RequestHeaderValueTransform(String.Empty, append: false)
                        }
                    };

                    if (context.Request.Cookies.TryGetValue(AuthenticationManager.AUTH_COOKIE, out string? authCookieValue))
                    {
                        // FIXME: This is currently also sent as cookie. Remove this and only send it as header.
                        requestHeaderTransforms.Add(
                            "X-Gatekeeper-Jwt-Assertion",
                            new RequestHeaderValueTransform(authCookieValue, append: false)
                        );
                    }

                    RequestProxyOptions proxyOptions = new RequestProxyOptions()
                    {
                        RequestTimeout = TimeSpan.FromSeconds(100),
                        Transforms = new Transforms(
                            copyRequestHeaders: true,
                            requestTransforms: Array.Empty<RequestParametersTransform>(),
                            requestHeaderTransforms: requestHeaderTransforms,
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
            bool exists = _proxyConfigProvider.GetRoutes().TryGetValue(context.Request.Host.Host, out MemorySingletonProxyConfigProvider.Route? route);
            if (!exists)
            {
                return null;
            }

            return route;
        }
    }
}
