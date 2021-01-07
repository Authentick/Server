using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Gatekeeper.Server.Services.Authentication.PasswordPolicy
{
    public class HIBP : IPasswordValidator<AppUser>
    {
        private readonly HIBPClient _hibpClient;

        public HIBP(
            HIBPClient hibpClient
        )
        {
            _hibpClient = hibpClient;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            bool isLeaked = await _hibpClient.IsBreachedAsync(password);

            if (isLeaked)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Password in HIBP breach", Code = "HIBPMatch" });
            }
            else
            {
                return IdentityResult.Success;
            }
        }
    }
}