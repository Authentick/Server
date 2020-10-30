using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Email;
using AuthServer.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hangfire;
using Microsoft.AspNetCore.Identity;

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
                        Success = true
                    };
                }
            }

            return new LoginReply { Success = false };
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
                    "foo@example.com",
                    "Foo",
                    "Test",
                    code
                ));

                return new RegisterReply { Success = true };
            }

            return new RegisterReply { Success = false };
        }

        public override Task<WhoAmIReply> WhoAmI(Empty request, ServerCallContext context)
        {
            var result = new WhoAmIReply { IsAuthenticated = true, UserId = "foobar" };
            return Task.FromResult(result);
        }
    }
}
