using System.Text;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Email;
using AuthServer.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthServer.Server.GRPC
{
    public class AuthService : AuthServer.Shared.Auth.AuthBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

        public override async Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {

            AppUser user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            IdentityResult result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                BackgroundJob.Enqueue<IEmailSender>(x => x.SendEmailAsync(
                    request.Email,
                    "Foo",
                    "Test",
                    "/email/confirm?userId=" + user.Id.ToString() + "&code=" + WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code))
                ));

                return new RegisterReply { Success = true };
            }

            return new RegisterReply { Success = false };
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

        public override Task<WhoAmIReply> WhoAmI(Empty request, ServerCallContext context)
        {
            string? userId = _userManager.GetUserId(context.GetHttpContext().User);

            WhoAmIReply result;
            if (userId != null)
            {
                result = new WhoAmIReply { IsAuthenticated = true, UserId = userId };
            }
            else
            {
                result = new WhoAmIReply { IsAuthenticated = false };
            }

            return Task.FromResult(result);
        }
    }
}
