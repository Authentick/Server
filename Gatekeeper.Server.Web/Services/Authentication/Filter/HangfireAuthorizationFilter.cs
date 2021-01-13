using Hangfire.Dashboard;

namespace AuthServer.Server.Services.Authentication.Filter
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // FIXME: This should be limited to admins
            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
