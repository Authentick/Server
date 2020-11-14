using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Models;
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

        public UsersService(
            UserManager userManager
            )
        {
            _userManager = userManager;
        }

        public override Task<UserListReply> ListUsers(Empty request, ServerCallContext context)
        {
            IEnumerable<AppUser> users = _userManager.GetAllUsers();

            UserListReply reply = new UserListReply { };

            foreach (AppUser user in users)
            {
                User userElement = new User { Id = user.Id.ToString(), Name = user.UserName };
                reply.Users.Add(userElement);
            }

            return Task.FromResult(reply);
        }
    }
}
