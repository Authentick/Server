using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Server.Services.Authentication.TwoFactorAuthenticators
{
    class TotpAuthenticatorProvider : AuthenticatorTokenProvider<AppUser>
    {
        private readonly AuthDbContext _authDbContext;
        public static readonly string ProviderName = "TotpAuthenticator";

        public TotpAuthenticatorProvider(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public override async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<AppUser> manager, AppUser user)
        {
            return true;
        }

        public override async Task<string> GenerateAsync(string purpose, UserManager<AppUser> manager, AppUser user)
        {
            return "asdf";
        }

        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<AppUser> manager, AppUser user)
        {
            return false;
        }
    }
}
