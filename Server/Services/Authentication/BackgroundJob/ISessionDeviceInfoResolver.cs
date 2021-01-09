using System;
using System.Threading.Tasks;

namespace Gatekeeper.Server.Services.Authentication.BackgroundJob
{
    public interface ISessionDeviceInfoResolver
    {
        Task ResolveForAuthSession(Guid authSessionId);
    }
}
