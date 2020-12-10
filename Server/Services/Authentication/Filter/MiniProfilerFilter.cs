using Microsoft.AspNetCore.Http;

namespace AuthServer.Server.Services.Authentication.Filter
{
    class MiniProfileFilter
    {
        public static bool CanSeeProfiler(HttpRequest request) {
            // FIXME: Limit to administrators
            return request.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}
