using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using static AuthServer.Shared.Auth;

namespace AuthServer.Client.Util
{
    class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthClient _authClient;
        private readonly InstallationStateProvider _installationStateProvider;

        public AuthStateProvider(
            AuthClient authClient,
            InstallationStateProvider installationStateProvider
            )
        {
            _authClient = authClient;
            _installationStateProvider = installationStateProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identityResult = await _authClient.WhoAmIAsync(new Google.Protobuf.WellKnownTypes.Empty());
            _installationStateProvider.IsInstalled = identityResult.IsInstalled;

            List<Claim> claims = new List<Claim>();
            if (identityResult.IsAuthenticated)
            {
                claims.Add(new Claim(ClaimTypes.Name, identityResult.UserId));
                foreach (string role in identityResult.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            ClaimsIdentity identity = new ClaimsIdentity();
            if (claims.Count > 0)
            {
                identity = new ClaimsIdentity(claims, "Gatekeeper Authentication");
            }

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public void StateHasChanged()
        {
            this.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
