using System.Net;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication;
using AuthServer.Server.Services.User;
using AuthServer.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace AuthServer.Server.GRPC
{
    public class AuthService : AuthServer.Shared.Auth.AuthBase
    {
        private readonly UserManager _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AuthDbContext _authDbContext;
        private readonly BruteforceManager _bruteforceManager;

        public AuthService(
            UserManager userManager,
            SignInManager<AppUser> signInManager,
            AuthDbContext authDbContext,
            BruteforceManager bruteforceManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authDbContext = authDbContext;
            _bruteforceManager = bruteforceManager;
        }

        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            AppUser? user = await _userManager.FindByNameAsync(request.UserId);
            HttpContext httpContext = context.GetHttpContext();

            IPAddress? ip = httpContext.Connection.RemoteIpAddress;
            if (ip == null)
            {
                return new LoginReply { State = LoginStateEnum.Failed };
            }

            StringValues userAgent;
            httpContext.Request.Headers.TryGetValue("User-Agent", out userAgent);

            if (userAgent.Count != 1)
            {
                return new LoginReply { State = LoginStateEnum.Failed };
            }

            bool isUserBlocked = false;
            if (user != null)
            {
                isUserBlocked = await _bruteforceManager.IsUserBlockedAsync(user);
            }
            bool isIpBlocked = await _bruteforceManager.IsIpBlockedAsync(ip);

            if (isUserBlocked || isIpBlocked)
            {
                return new LoginReply
                {
                    State = LoginStateEnum.Blocked,
                };
            }

            if (user == null)
            {
                await _bruteforceManager.MarkInvalidLoginAttemptAsync(
                    ip,
                    userAgent[0],
                    request.UserId
                );

                return new LoginReply
                {
                    State = LoginStateEnum.Failed
                };
            }

            Microsoft.AspNetCore.Identity.SignInResult result =
                await _signInManager.PasswordSignInAsync(user, request.Password, false, false);

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

            await _bruteforceManager.MarkInvalidLoginAttemptAsync(
                ip,
                userAgent[0],
                request.UserId
            );

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
            AppUser? user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            WhoAmIReply result = new WhoAmIReply();

            SystemSetting? installedSetting = await _authDbContext.SystemSettings
                .SingleOrDefaultAsync(s => s.Name == "installer.is_installed" && s.Value == "true");
            result.IsInstalled = (installedSetting != null);

            if (user != null)
            {
                result.IsAuthenticated = true;
                result.UserId = user.Id.ToString();
                result.Roles.AddRange(await _userManager.GetRolesAsync(user));
            }
            else
            {
                result.IsAuthenticated = false;
            }

            return result;
        }
    }
}
