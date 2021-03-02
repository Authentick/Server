using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Authentick.DotNetSaml.Certificate
{
    public class PemKeyConverter
    {
        public string GetPrivateKey(X509Certificate2 certificate)
        {
            AsymmetricAlgorithm? key = certificate.GetRSAPrivateKey();
            if (key == null)
            {
                throw new Exception("Cannot get RSA key");
            }

            string header = "-----BEGIN PRIVATE KEY-----";
            string footer = "-----END PRIVATE KEY-----";
            string privateKey = Convert.ToBase64String(key.ExportPkcs8PrivateKey());

            return $"{header}\n{privateKey}\n{footer}";
        }

        public string GetPublicKey(X509Certificate2 certificate)
        {
            AsymmetricAlgorithm? key = certificate.GetRSAPrivateKey();
            if (key == null)
            {
                throw new Exception("Cannot get RSA key");
            }

            string header = "-----BEGIN CERTIFICATE-----";
            string footer = "-----END CERTIFICATE-----";
            string publicKey = Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks);

            return $"{header}\n{publicKey}\n{footer}";
        }

        public X509Certificate2 GetCertificate(string publicKey, string privateKey)
        {
            return X509Certificate2.CreateFromPem(publicKey, privateKey);
        }
    }
}
