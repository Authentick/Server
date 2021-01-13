using System;
using System.Collections.Generic;
using AuthServer.Shared.Admin;

namespace Gatekeeper.Client.Pages.Admin.Apps.Setup.Steps
{
    public class AuthenticationMethodsProvider
    {
        public IEnumerable<IAuthMethod> GetAuthMethods(HostingType hostingType)
        {
            switch (hostingType)
            {
                case HostingType.WebGeneric:
                    return new List<IAuthMethod>(){
                        new OpenIDConnectAuthMethod(),
                        new LDAPAuthMethod(),
                    };

                case HostingType.WebGatekeeperProxy:
                    return new List<IAuthMethod>() {
                        new GatkeeperProxyAuthMethod(),
                        new OpenIDConnectAuthMethod(),
                        new LDAPAuthMethod(),
                    };
                case HostingType.NonWeb:
                    return new List<IAuthMethod>() {
                        new LDAPAuthMethod(),
                    };
                default:
                    throw new NotImplementedException();
            }
        }

        public class GatkeeperProxyAuthMethod : IAuthMethod
        {
            public string Name => "No additional authentication method";
            public string Description => "Gatekeeper will act as a reverse proxy for all incoming traffic and authorize users for you.";
            public List<string> Advantages => new List<string>() {
                "Your application will only be accessible to authorized users.",
                "Security issues in backend applications are not accessible to the public.",
                "Revoked sessions apply instantly.",
            };

            public List<string> Disadvantages => new List<string>() {
                "JWT authentication has to be implemented to automatically login users.",
            };
        }

        public class OpenIDConnectAuthMethod : IAuthMethod
        {
            public string Name => "OpenID Connect";
            public string Description => "Gatekeeper will act as an OpenID Connect server.";
            public List<string> Advantages => new List<string>() {
                "SaaS applications widely support OpenID.",
                "Public access possible if your application supports it.",
            };

            public List<string> Disadvantages => new List<string>() {
                "Session revocation is not instant.",
            };
        }

        public class LDAPAuthMethod : IAuthMethod
        {
            public string Name => "LDAP";
            public string Description => "Gatekeeper will act as an LDAP server.";
            public List<string> Advantages => new List<string>() {
                "Non-web based applications often support LDAP. (e.g. Postfix)",
            };

            public List<string> Disadvantages => new List<string>() {
                "Session revocation is not instant.",
            };
        }

        public interface IAuthMethod
        {
            string Name { get; }
            string Description { get; }
            List<string> Advantages { get; }
            List<string> Disadvantages { get; }
        }
    }
}
