using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Shared.Security;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Server.GRPC.Security
{
    [Authorize]
    public class SessionsService : AuthServer.Shared.Security.Sessions.SessionsBase
    {
        private readonly UserManager<AppUser> _userManager;

        public SessionsService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public override Task<InvalidateSessionReply> InvalidateSession(InvalidateSessionRequest request, ServerCallContext context)
        {
            return base.InvalidateSession(request, context);
        }

        public override Task<SessionListReply> ListActiveSessions(Empty request, ServerCallContext context)
        {
            return base.ListActiveSessions(request, context);
        }

        public override Task<SessionListReply> ListInactiveSessions(Empty request, ServerCallContext context)
        {
            return base.ListInactiveSessions(request, context);
        }
    }
}