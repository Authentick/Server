using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Authentick.DotNetSaml.Certificate
{
    public class CertificateFactory
    {
        public X509Certificate2 GetNewCertificate()
        {
            using (RSA rsa = RSA.Create(4096))
            {
                CertificateRequest request = new CertificateRequest(
                    new X500DistinguishedName($"CN=Authentick"), 
                    rsa, 
                    HashAlgorithmName.SHA256, 
                    RSASignaturePadding.Pkcs1
                );
                X509Certificate2 certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddYears(10)));

                return certificate;
            }
        }
    }
}
