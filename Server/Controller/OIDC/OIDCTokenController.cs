using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto.OIDC;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Controller.OIDC
{
    [Route("connect/token")]
    public class OIDCTokenController : ControllerBase
    {
        private readonly OIDCKeyManager _oidcKeyManager;
        private readonly AuthDbContext _authDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OIDCTokenController(
            OIDCKeyManager oidcKeyManager,
            AuthDbContext authDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _oidcKeyManager = oidcKeyManager;
            _authDbContext = authDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<OidcTokenReply> TokenReply(string code, string client_id, string client_secret)
        {
            // FIXME: should use time-constant comparison
            OIDCSession session = await _authDbContext.OIDCSessions
            .Where(o => o.OIDCAppSettings.ClientSecret == client_secret && o.OIDCAppSettings.ClientId == client_id)
                .Where(o => o.Id == new Guid(code))
                .Include(s => s.User)
                .Include(s => s.OIDCAppSettings)
                .SingleAsync();

            var rsaKey = _oidcKeyManager.GetKey();

            string protocolString = (_httpContextAccessor.HttpContext.Request.IsHttps ? "https://" : "http://");
            string issuer = protocolString + _httpContextAccessor.HttpContext.Request.Host;

            var json = new JwtBuilder()
                .WithAlgorithm(new RS256Algorithm(rsaKey, rsaKey))
                // FIXME
                .Issuer(issuer)
                .Subject(session.User.Id.ToString())
                .AddClaim(ClaimName.Nonce, session.Nonce)
                .Audience(session.OIDCAppSettings.ClientId)
                .AddHeader(HeaderName.KeyId, "1")
                .IssuedAt(DateTime.UtcNow)
                .ExpirationTime(DateTime.UtcNow.AddHours(10))
                .Encode();

            // fixme: tokens should expire
            return new OidcTokenReply
            {
                AccessToken = code,
                TokenType = "Bearer",
                RefreshToken = code,
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
