using AuthServer.Server.Models;

namespace Gatekeeper.Server.Web.Services.Alerts.Types
{
    public interface IUserAlert : IAlert 
    {
        AppUser TargetUser { get; set; }
    }
}
