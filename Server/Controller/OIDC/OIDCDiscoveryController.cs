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
            string protocolString = (_contextAccessor.HttpContext.Request.IsHttps) ? "https" : "http";

            return new DiscoveryDocument
            {
                Issuer = protocolString + "://" + host.Host,
                AuthorizationEndpoint = protocolString + "://" + host.Host + "/connect/authorize",
                TokenEndpoint = protocolString + "://" + host.Host + "/connect/token",
                JwksUri = protocolString + "://" + host.Host + "/.well-known/jwks.json",
                ResponseTypesSupported = new List<string>() { "code" },
                SubjectTypesSupported = new List<string>() { "pairwise" },
                IdTokenSigningAlgValuesSupported = new List<string>() { "RS256" },
                TokenEndpointAuthMethodsSupported = new List<string>() { "client_secret_post" },
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

            [JsonPropertyName("token_endpoint_auth_methods_supported")]
            public IEnumerable<string> TokenEndpointAuthMethodsSupported { get; set; } = null!;
        }
    }
}
