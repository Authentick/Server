using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Server.Controller.OIDC
{
    [Route(".well-known/openid-configuration")]
    public class OIDCDiscoveryController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public OIDCDiscoveryController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        [HttpGet]
        public DiscoveryDocument GetDocument()
        {
            HostString host = _contextAccessor.HttpContext.Request.Host;

            return new DiscoveryDocument
            {
                Issuer = "https://" + host.Host,
                AuthorizationEndpoint = "https://" + host.Host + "/connect/authorize",
                TokenEndpoint = "https://" + host.Host + "/connect/token",
                JwksUri = "https://" + host.Host + "/jwks.json",
                ResponseTypesSupported = new List<string>() { "code" },
                SubjectTypesSupported = new List<string>() { "pairwise" },
                IdTokenSigningAlgValuesSupported = new List<string>() { "HS256" },
            };
        }

        public class DiscoveryDocument
        {
            [JsonPropertyName("issuer")]
            public string Issuer { get; set; } = null!;

            [JsonPropertyName("authorization_endpoint")]
            public string AuthorizationEndpoint { get; set; } = null!;

            [JsonPropertyName("token_endpoint")]
            public string TokenEndpoint { get; set; } = null!;

            [JsonPropertyName("jwks_uri")]
            public string JwksUri { get; set; } = null!;

            [JsonPropertyName("response_types_supported")]
            public IEnumerable<string> ResponseTypesSupported { get; set; } = null!;

            [JsonPropertyName("subject_types_supported")]
            public IEnumerable<string> SubjectTypesSupported { get; set; } = null!;

            [JsonPropertyName("id_token_signing_alg_values_supported")]
            public IEnumerable<string> IdTokenSigningAlgValuesSupported { get; set; } = null!;
        }
    }
}