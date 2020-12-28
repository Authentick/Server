using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Server.Services.ReverseProxy.Authentication
{
    class SingleSignOnHandler
    {
        public const string AUTH_PARAM_NAME = "gatekeeper_proxy_sso";
        private readonly IDataProtector _gatekeeperProxySsoSessionProtector;

        public SingleSignOnHandler(IDataProtectionProvider dataProtectionProvider)
        {
            _gatekeeperProxySsoSessionProtector = dataProtectionProvider.CreateProtector("GATEKEEPER_PROXY_SSO");
        }

        public bool IsAuthRequest(HttpContext context) {
            HttpRequest request = context.Request;
            if(request.Method.ToUpper() == "GET" && request.Path == "/gatekeeper-proxy-sso") {
                bool hasAuthParam = request.Query.ContainsKey(AUTH_PARAM_NAME);
                
                return hasAuthParam;
            }

            return false;
        }

        public void Handle(HttpContext context) {
            HttpRequest request = context.Request;
            string authToken = request.Query[AUTH_PARAM_NAME];

            string decryptedId = _gatekeeperProxySsoSessionProtector.Unprotect(authToken);

            CookieOptions cookieOptions = new CookieOptions {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
            };

            context.Response.Cookies.Append(AuthenticationManager.AUTH_COOKIE, authToken, cookieOptions);
            context.Response.Redirect("/");
        }
    }
}
