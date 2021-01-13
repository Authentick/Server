using AuthServer.Server.Services.Crypto;
using Moq;
using Xunit;

namespace AuthServer.Server.Tests.Services.Crypto
{
    public class HasherTest
    {
        [Fact]
        public void TestValidHash()
        {
            string input = "ThisIsTheStringToHash";

            Hasher hasher = new Hasher();
            string hash = hasher.Hash(input);

            Assert.True(hasher.VerifyHash(hash, input));
        }

        [Fact]
        public void TestInalidHash()
        {
            string input = "ThisIsTheStringToHash";

            Hasher hasher = new Hasher();
            string hash = hasher.Hash(input);

            Assert.False(hasher.VerifyHash(hash, "AnotherString"));
        }
    }
}
