using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using AuthServer.Server.Services.Crypto.OIDC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Server.Controller.OIDC
{
    [Route(".well-known/jwks.json")]
    public class JwksController : ControllerBase
    {
        private readonly OIDCKeyManager _oidcKeyManager;

        public JwksController(OIDCKeyManager oidcKeyManager)
        {
            _oidcKeyManager = oidcKeyManager;
        }

        [HttpGet]
        public JwksReply Get()
        {
            RSA key = _oidcKeyManager.GetKey();
            RSAParameters rsaParameters = key.ExportParameters(false);

            return new JwksReply
            {
                Keys = new List<JwksKey>(){
                    new JwksKey{
                        KeyId = "1",
                        Use = "sig",
                        KeyType = "RSA",
                        Algorithm = "RS256",
                        PublicExponent = Base64UrlEncoder.Encode(rsaParameters.Exponent),
                        PublicModulus = Base64UrlEncoder.Encode(rsaParameters.Modulus),
                    },
                },
            };
        }

        public class JwksReply
        {
            [JsonPropertyName("keys")]
            public List<JwksKey> Keys { get; set; } = null!;
        }

        public class JwksKey
        {
            [JsonPropertyName("kty")]
            public string KeyType { get; set; } = null!;

            [JsonPropertyName("kid")]
            public string KeyId { get; set; } = null!;

            [JsonPropertyName("alg")]
            public string Algorithm { get; set; } = null!;

            [JsonPropertyName("e")]
            public string PublicExponent { get; set; } = null!;

            [JsonPropertyName("n")]
            public string PublicModulus { get; set; } = null!;

            [JsonPropertyName("use")]
            public string Use { get; set; } = null!;
        }
    }
}
