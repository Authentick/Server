namespace AuthServer.Server.Models
{
    public class OpenIdDiscoveryModel {
        public string issuer { get; set; } = null!;
        public string authorization_endpoint { get; set; } = null!;
        public string jwks_uri { get; set; } = null!;
    }
}