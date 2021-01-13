using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using AuthServer.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC
{
    [Authorize]
    public class UserProfileService : AuthServer.Shared.UserProfile.UserProfileBase
    {
        private readonly AuthDbContext _dbContext;
        private readonly UserManager _userManager;

        public UserProfileService(
            AuthDbContext context,
            UserManager userManager)
        {
            _dbContext = context;
            _userManager = userManager;
        }

        public override async Task<UserReply> GetUser(Empty request, ServerCallContext context)
        {
            string userId = _userManager.GetUserId(context.GetHttpContext().User);

            AppUser user = await _dbContext.Users
                .Include(u => u.Groups)
                .SingleAsync(u => u.Id == new Guid(userId));

            UserReply reply = new UserReply
            {
                Username = user.UserName,
                Email = user.Email,
            };

            foreach (UserGroup group in user.Groups)
            {
                reply.GroupNames.Add(group.Name);
            }

            return reply;
        }
    }
}
