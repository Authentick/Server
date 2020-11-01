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
    public class SettingsService : AuthServer.Shared.Security.Settings.SettingsBase
    {
        private readonly UserManager<AppUser> _userManager;

        public SettingsService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public override Task<AddNewAuthenticatorAppReply> AddNewAuthenticatorApp(AddNewAuthenticatorAppRequest request, ServerCallContext context)
        {
            return base.AddNewAuthenticatorApp(request, context);
        }

        public override async Task<ChangePasswordReply> ChangePassword(ChangePasswordRequest request, ServerCallContext context)
        {
            AppUser? user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            if (user == null)
            {
                return new ChangePasswordReply { Success = false };
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return new ChangePasswordReply { Success = true };
            }

            return new ChangePasswordReply { Success = false };
        }

        public override Task<TwoFactorListReply> ListAuthenticatorApps(Empty request, ServerCallContext context)
        {
            return base.ListAuthenticatorApps(request, context);
        }

        public override Task<TwoFactorListReply> ListAuthenticatorKeys(Empty request, ServerCallContext context)
        {
            return base.ListAuthenticatorKeys(request, context);
        }

        public override Task<RemoveAuthenticatorReply> RemoveAuthenticator(RemoveAuthenticatorRequest request, ServerCallContext context)
        {
            return base.RemoveAuthenticator(request, context);
        }
    }
}