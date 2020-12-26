using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Server.Services.ReverseProxy.Authentication
{
    class AuthenticationHandler
    {
        const string AUTH_PARAM_NAME = "GATEKEEPER_PROXY_SSO";
        const string REDIRECT_PARAM_NAME = "GATEKEEPER_PROXY_SSO_REDIRECT";
        private readonly IDataProtector _gatekeeperProxySsoSessionProtector;

        public AuthenticationHandler(IDataProtectionProvider dataProtectionProvider)
        {
            _gatekeeperProxySsoSessionProtector = dataProtectionProvider.CreateProtector("GATEKEEPER_PROXY_SSO");
        }

        public bool IsAuthRequest(HttpContext context) {
            HttpRequest request = context.Request;
            if(request.Method.ToUpper() == "POST" && request.Path == "/gatekeeper-proxy-sso") {
                bool hasAuthParam = request.Form.ContainsKey(AUTH_PARAM_NAME);
                bool hasRedirectUri = request.Form.ContainsKey(REDIRECT_PARAM_NAME);
                
                return hasAuthParam && hasRedirectUri;
            }

            return false;
        }

        public void Handle(HttpContext context) {
            HttpRequest request = context.Request;
            string authToken = request.Form[AUTH_PARAM_NAME];

            string decryptedId = _gatekeeperProxySsoSessionProtector.Unprotect(authToken);

            CookieOptions cookieOptions = new CookieOptions{
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
            };

            context.Response.Cookies.Append("gatekeeper.proxy.sso", authToken, cookieOptions);
            // FIXME: Open Redirect
            context.Response.Redirect(request.Form[REDIRECT_PARAM_NAME]);
        }
    }
}
