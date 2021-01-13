using System.Security.Cryptography;

namespace AuthServer.Server.Services.Crypto.OIDC
{
    public class OIDCKeyManager
    {
        private readonly ConfigurationProvider _configurationProvider;
        private string _configKey = "oidc.rsa.key";

        public OIDCKeyManager(ConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public RSA GetKey()
        {
            string key = "";
            bool exists = _configurationProvider.TryGet(_configKey, out key);

            if (!exists)
            {
                RSA rsaKey = GenerateKey();
                key = rsaKey.ToXmlString(true);
                _configurationProvider.Set(_configKey, key);
            }

            return GetKeyFromXmlString(key);
        }

        private RSA GetKeyFromXmlString(string xmlKey)
        {

            RSA returnKey = RSA.Create();
            returnKey.FromXmlString(xmlKey);

            return returnKey;
        }

        private RSA GenerateKey()
        {
            return RSA.Create(2048);
        }
    }
}
