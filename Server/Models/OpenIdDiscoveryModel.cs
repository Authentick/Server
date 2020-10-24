namespace AuthServer.Server.Models
{
    public class OpenIdDiscoveryModel {
        public string issuer { get; set; }
        public string authorization_endpoint { get; set; }
        public string jwks_uri { get; set; }
    }
}