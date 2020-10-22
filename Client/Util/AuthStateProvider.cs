using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using static AuthServer.Shared.Auth;

namespace AuthServer.Client.Util
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthClient _authClient;

        public AuthStateProvider(AuthClient authClient)
        {
            _authClient = authClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identityResult = await _authClient.WhoAmIAsync(new Google.Protobuf.WellKnownTypes.Empty());

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, identityResult.UserId),
            }, "Fake authentication type");

            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
    }
}