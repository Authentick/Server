using System.Security.Cryptography.X509Certificates;
using Authentick.DotNetSaml.Certificate;
using Xunit;

namespace Authentick.DotNetSaml.Tests.Certificate
{
    public class CertificateFactoryTest
    {
        [Fact]
        public void TestGetNewCertificate()
        {
            CertificateFactory factory  = new CertificateFactory();
            X509Certificate2 cert = factory.GetNewCertificate();

            Assert.Equal(
                4096,
                cert.GetRSAPrivateKey().KeySize
            );
            Assert.Equal(
                "CN=Authentick",
                cert.SubjectName.Name
            );
        }
    }
}
