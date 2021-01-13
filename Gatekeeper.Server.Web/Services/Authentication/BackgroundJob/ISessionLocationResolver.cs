using System;
using System.Threading.Tasks;

namespace Gatekeeper.Server.Services.Authentication.BackgroundJob
{
    public interface ISessionLocationResolver
    {
        Task ResolveForAuthSessionIp(Guid authSessionIpId);
    }
}
