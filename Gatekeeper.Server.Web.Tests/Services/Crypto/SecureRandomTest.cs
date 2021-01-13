using AuthServer.Server.Services.Crypto;
using Xunit;

namespace AuthServer.Server.Tests.Services.Crypto
{
    public class SecureRandomTest
    {
        [Fact]
        public void TestGetRandomString()
        {
            SecureRandom random = new SecureRandom();

            Assert.Equal(23, random.GetRandomString(23).Length);
        }
    }
}
