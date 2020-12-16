using System.Collections.Generic;

namespace AuthServer.Client.Pages.Admin.Apps.Setup.Steps
{
    class DirectoryMethodsProvider
    {
        public IEnumerable<IDirectoryMethod> GetDirectoryMethods()
        {
            return new List<IDirectoryMethod>(){
                new NoDirectoryServiceMethod(),
                new SCIMDirectoryServiceMethod(),
                new LDAPDirectoryServiceMethod(),
            };
        }

        public class NoDirectoryServiceMethod : IDirectoryMethod
        {
            public string Name => "No Directory Service";
            public string Description => "Enable no directory services for this app.";
            public List<string> Advantages => new List<string>() { };

            public List<string> Disadvantages => new List<string>() {
                "The configured app won't have access to an up-to-date list of users.",
            };
        }

        public class SCIMDirectoryServiceMethod : IDirectoryMethod
        {
            public string Name => "SCIM";
            public string Description => "Gatekeeper will act as SCIM client.";
            public List<string> Advantages => new List<string>() {
                "Gatekeeper will push updates instantly to the application.",
            };

            public List<string> Disadvantages => new List<string>() { };
        }

        public class LDAPDirectoryServiceMethod : IDirectoryMethod
        {
            public string Name => "LDAP";
            public string Description => "Gatekeeper will act as an LDAP server.";
            public List<string> Advantages => new List<string>() {
                "Many self-hosted applications widely support LDAP.",
            };

            public List<string> Disadvantages => new List<string>() {
                "Some LDAP supported applications cache LDAP results for performance reasons, resulting in delayed updates.",
            };
        }

        public interface IDirectoryMethod
        {
            string Name { get; }
            string Description { get; }
            List<string> Advantages { get; }
            List<string> Disadvantages { get; }
        }
    }
}
