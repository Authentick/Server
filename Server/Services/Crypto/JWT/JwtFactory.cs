using System.Security.Cryptography;
using AuthServer.Server.Services.Crypto.OIDC;
using JWT.Algorithms;
using JWT.Builder;

namespace AuthServer.Server.Services.Crypto.JWT
{
    public class JwtFactory
    {
        private readonly OIDCKeyManager _oidcKeyManager;

        public JwtFactory(OIDCKeyManager oidcKeyManager)
        {
            _oidcKeyManager = oidcKeyManager;
        }

        public JwtBuilder Build()
        {
            RSA rsaKey = _oidcKeyManager.GetKey();

            return new JwtBuilder()
                .WithAlgorithm(new RS256Algorithm(rsaKey, rsaKey));
        }
    }
}
