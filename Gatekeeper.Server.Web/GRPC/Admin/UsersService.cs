using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using AuthServer.Shared.Admin;
using Gatekeeper.Server.Services.FileStorage;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Server.GRPC.Admin
{
    [Authorize(Policy = "SuperAdministrator")]
    public class UsersService : AuthServer.Shared.Admin.Users.UsersBase
    {
        private readonly UserManager _userManager;
        private readonly ProfileImageManager _profileImageManager;
        private const string ADMIN_ROLE = "admin";

        public UsersService(
            UserManager userManager,
            ProfileImageManager profileImageManager
            )
        {
            _userManager = userManager;
            _profileImageManager = profileImageManager;
        }

        public override async Task<ChangeAdminStateResponse> ChangeAdminState(ChangeAdminStateRequest request, ServerCallContext context)
        {
            AppUser user = await _userManager.FindByIdAsync(request.Id);
            if (request.IsAdmin)
            {
                await _userManager.AddToRoleAsync(user, ADMIN_ROLE);
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, ADMIN_ROLE);
            }

            return new ChangeAdminStateResponse
            {
                Success = true,
            };
        }

        public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            AppUser user = new AppUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = true,
            };

            IdentityResult result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new CreateUserResponse
                {
                    Success = false,
                    Error = result.Errors.First().Description,
                };
            }

            return new CreateUserResponse
            {
                Success = true,
            };
        }

        public override async Task<UserListReply> ListUsers(Empty request, ServerCallContext context)
        {
            IEnumerable<AppUser> users = _userManager.GetAllUsers();

            UserListReply reply = new UserListReply { };

            foreach (AppUser user in users)
            {
                User userElement = new User
                {
                    Id = user.Id.ToString(),
                    Name = user.UserName,
                    Email = user.Email,
                    IsAdmin = await _userManager.IsInRoleAsync(user, ADMIN_ROLE),
                    IsEnabled = true,
                    HasPicture = _profileImageManager.HasProfileImage(user.Id),
                };
                reply.Users.Add(userElement);
            }

            return reply;
        }

        public override string? ToString()
        {
            return base.ToString();
        }
    }
}
