using System.Threading.Tasks;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.User;
using AuthServer.Shared.Admin;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Server.GRPC.Admin
{
    [Authorize]
    public class UsersService : AuthServer.Shared.Admin.Users.UsersBase
    {
        private readonly UserManager _userManager;
        private readonly SessionManager _sessionManager;

        public UsersService(
            UserManager userManager,
            SessionManager sessionManager
            )
        {
            _userManager = userManager;
            _sessionManager = sessionManager;
        }

        public override Task<UserListReply> ListUsers(Empty request, ServerCallContext context)
        {
            return base.ListUsers(request, context);
        }
    }
}
