using System.Collections.Generic;

namespace Gatekeeper.Client.Pages.Admin.Apps.Setup.Steps
{
    public record AuthIdentityChoiceStep
    {
        public List<AuthServiceEnum> AuthServices { get; init; } = new List<AuthServiceEnum>();
        public List<IdentityServiceEnum> IdentityServices { get; init; } = new List<IdentityServiceEnum>();

        public enum AuthServiceEnum
        {
            IdentityAwareProxy = 1,
            OIDC = 2,
            LDAP = 3,
        }

        public enum IdentityServiceEnum
        {
            SCIM = 1,
            LDAP = 2,
        }
    }
}
