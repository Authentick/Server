using System;
using System.Text.Json.Serialization;
using AuthServer.Server.Services.Crypto.OIDC;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Server.Controller.OIDC
{
    [Route("connect/token")]
    public class OIDCTokenController : ControllerBase
    {
        private readonly OIDCKeyManager _oidcKeyManager;

        public OIDCTokenController(OIDCKeyManager oidcKeyManager)
        {
            _oidcKeyManager = oidcKeyManager;
        }

        [HttpPost]
        public OidcTokenReply TokenReply()
        {
            var rsaKey = _oidcKeyManager.GetKey();

            var json = new JwtBuilder()
                .WithAlgorithm(new RS256Algorithm(rsaKey, rsaKey))
                .Issuer("https://our.gatekeeper.page")
                .Subject("UserId")
                .Audience("TODO Audience")
                .AddHeader(HeaderName.KeyId, "1")
                .IssuedAt(DateTime.UtcNow)
                .ExpirationTime(DateTime.UtcNow.AddHours(10))
                .Encode();

            return new OidcTokenReply
            {
                AccessToken = "asdf",
                TokenType = "Bearer",
                RefreshToken = "refreshtoken",
                ExpiresIn = 3600,
                IdToken = json,
            };
        }

        public class OidcTokenReply
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = null!;

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = null!;

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; } = null!;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; } = null!;
        }
    }
}
