using System.Security.Cryptography;
using AuthServer.Server.Services;
using AuthServer.Server.Services.Crypto.OIDC;
using Xunit;

namespace AuthServer.Server.Tests.Services.Authentication.Session
{
    public class OIDCKeyManagerTest : IClassFixture<SharedDatabaseFixture>
    {
        public OIDCKeyManagerTest(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        public SharedDatabaseFixture Fixture { get; }

        [Fact]
        public void TestGetKey()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    ConfigurationProvider provider = new ConfigurationProvider(context);
                    OIDCKeyManager manager = new OIDCKeyManager(provider);

                    RSA initialKeyLoading = manager.GetKey();
                    RSA secondKeyLoading = manager.GetKey();

                    Assert.Equal(initialKeyLoading.ToXmlString(true), secondKeyLoading.ToXmlString(true));
                    Assert.Equal(2048, initialKeyLoading.KeySize); 
                }
            }
        }
    }
}
