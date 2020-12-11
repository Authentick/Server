using System.Text;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using AuthServer.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC
{
    public class AuthService : AuthServer.Shared.Auth.AuthBase
    {
        private readonly UserManager _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AuthDbContext _authDbContext;

        public AuthService(
            UserManager userManager,
            SignInManager<AppUser> signInManager,
            AuthDbContext authDbContext
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authDbContext = authDbContext;
        }

        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            AppUser user = await _userManager.FindByNameAsync(request.UserId);

            if (user != null)
            {
                Microsoft.AspNetCore.Identity.SignInResult result =
                    await _signInManager.PasswordSignInAsync(user, request.Password, true, false);

                if (result.Succeeded)
                {
                    return new LoginReply
                    {
                        State = LoginStateEnum.Success
                    };
                }
                else if (result.RequiresTwoFactor)
                {
                    return new LoginReply
                    {
                        State = LoginStateEnum.TwoFactorRequired
                    };
                }
            }

            return new LoginReply
            {
                State = LoginStateEnum.Failed
            };
        }

        public override async Task<VerifyAuthenticatorReply> VerifyAuthenticatorToken(VerifyAuthenticatorTokenRequest request, ServerCallContext context)
        {
            AppUser? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return new VerifyAuthenticatorReply { Success = false };
            }

            SignInResult result = await _signInManager.TwoFactorAuthenticatorSignInAsync(request.Token, false, false);

            if (result.Succeeded)
            {
                return new VerifyAuthenticatorReply { Success = true };
            }

            return new VerifyAuthenticatorReply { Success = false };
        }

        public override async Task<VerifyEmailReply> VerifyEmail(VerifyEmailRequest request, ServerCallContext context)
        {
            AppUser user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                return new VerifyEmailReply { Success = false };
            }

            string code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return new VerifyEmailReply { Success = true };
            }

            return new VerifyEmailReply { Success = false };
        }

        public override async Task<WhoAmIReply> WhoAmI(Empty request, ServerCallContext context)
        {
            string? userId = _userManager.GetUserId(context.GetHttpContext().User);

            WhoAmIReply result = new WhoAmIReply();

            SystemSetting? installedSetting = await _authDbContext.SystemSettings
                .SingleOrDefaultAsync(s => s.Name == "installer.is_installed" && s.Value == "true");
            result.IsInstalled = (installedSetting != null);

            if (userId != null)
            {
                result.IsAuthenticated = true;
                result.UserId = userId;
            }
            else
            {
                result.IsAuthenticated = false;
            }

            return result;
        }
    }
}
