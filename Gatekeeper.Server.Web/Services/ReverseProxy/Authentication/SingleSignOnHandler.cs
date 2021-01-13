using Microsoft.AspNetCore.Http;

namespace AuthServer.Server.Services.ReverseProxy.Authentication
{
    class SingleSignOnHandler
    {
        public const string AUTH_PARAM_NAME = "gatekeeper_proxy_sso";

        public bool IsAuthRequest(HttpContext context) {
            HttpRequest request = context.Request;
            request.Cookies.TryGetValue("gatekeeper.csrf", out string? gatekeeperCsrfCookie);

            if(
                request.Method.ToUpper() == "POST" && 
                request.Path == "/gatekeeper-proxy-sso" && 
                request.Form.ContainsKey(AUTH_PARAM_NAME) &&
                request.Form.ContainsKey("gatekeeper_proxy_csrf") &&
                request.Form["gatekeeper_proxy_csrf"] == gatekeeperCsrfCookie
                ) {                
                return true;
            }

            return false;
        }

        public void Handle(HttpContext context) {
            HttpRequest request = context.Request;
            string authToken = request.Form[AUTH_PARAM_NAME];

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
