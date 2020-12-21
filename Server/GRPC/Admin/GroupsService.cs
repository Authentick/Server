using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Shared.Admin;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC.Admin
{
    [Authorize(Policy="SuperAdministrator")]
    public class GroupsService : AuthServer.Shared.Admin.Groups.GroupsBase
    {
        private readonly AuthDbContext _authDbContext;

        public GroupsService(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public override async Task<AddUserToGroupResponse> AddUserToGroup(AddUserToGroupRequest request, ServerCallContext context)
        {
            Guid groupId = new Guid(request.GroupId);
            UserGroup group = await _authDbContext.UserGroup
                .Include(ug => ug.Members)
                .SingleAsync(ug => ug.Id == groupId);

            Guid userId = new Guid(request.UserId);
            AppUser user = await _authDbContext.Users.SingleAsync(u => u.Id == userId);
            group.Members.Add(user);

            await _authDbContext.SaveChangesAsync();

            return new AddUserToGroupResponse
            {
                Success = true,
            };
        }

        public override async Task<GroupCreationResponse> CreateGroup(GroupCreationRequest request, ServerCallContext context)
        {
            UserGroup group = new UserGroup
            {
                Name = request.Name,
            };
            _authDbContext.Add(group);
            await _authDbContext.SaveChangesAsync();

            return new GroupCreationResponse
            {
                GroupId = group.Id.ToString(),
                Success = true,
            };
        }

        public override async Task<GroupDetailResponse> GetGroupDetails(GroupDetailRequest request, ServerCallContext context)
        {
            GroupDetailResponse groupDetailResponse = new GroupDetailResponse { };

            Guid groupId = new Guid(request.Id);
            UserGroup userGroup = await _authDbContext.UserGroup
                .Include(ug => ug.Members)
                .SingleAsync(ug => ug.Id == groupId);

            groupDetailResponse.Name = userGroup.Name;
            foreach (AppUser user in userGroup.Members)
            {
                GroupMember groupMember = new GroupMember
                {
                    Id = user.Id.ToString(),
                    Name = user.UserName
                };
                groupDetailResponse.Members.Add(groupMember);
            }

            return groupDetailResponse;
        }

        public override async Task<GroupsListReply> ListGroups(Empty request, ServerCallContext context)
        {
            GroupsListReply reply = new GroupsListReply { };

            ICollection<UserGroup> userGroups = await _authDbContext.UserGroup.ToListAsync();

            foreach (UserGroup group in userGroups)
            {
                Group groupEntry = new Group
                {
                    Id = group.Id.ToString(),
                    Name = group.Name
                };
                reply.Groups.Add(groupEntry);
            }

            return reply;
        }

        public override async Task<RemoveUserFromGroupResponse> RemoveUserFromGroup(RemoveUserFromGroupRequest request, ServerCallContext context)
        {
            GroupDetailResponse groupDetailResponse = new GroupDetailResponse { };

            Guid userId = new Guid(request.UserId);
            AppUser user = await _authDbContext.Users
                .SingleAsync(u => u.Id == userId);

            Guid groupId = new Guid(request.GroupId);
            UserGroup userGroup = await _authDbContext.UserGroup
                .Include(ug => ug.Members)
                .SingleAsync(g => g.Id == groupId);

            userGroup.Members.Remove(user);
            await _authDbContext.SaveChangesAsync();

            return new RemoveUserFromGroupResponse { Success = true };
        }
    }
}
