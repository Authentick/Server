using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Gatekeeper.Server.Services.GeoLocation
{
    public class GeoLocationManager
    {
        private readonly IHttpClientFactory _clientFactory;

        public GeoLocationManager(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private class ApiReply
        {
            public bool Success { get; set; }
            public string? City { get; set; }
            public string? CountryCode { get; set; }
        }

        public async Task<GeoLocation?> TryResolveLocationAsync(IPAddress ip)
        {
            HttpClient client = _clientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = "geoip-services.gatekeeper.page";
            uriBuilder.Path = ip.ToString();

            Uri uri = uriBuilder.Uri;

            try
            {
                ApiReply reply = await client.GetFromJsonAsync<ApiReply>(uri);
                if (reply.Success)
                {
                    GeoLocation geoLocation = new GeoLocation
                    {
                        City = reply.City,
                        CountryCode = reply.CountryCode,
                    };
                    return geoLocation;
                }
            }
            catch { }

            return null;
        }
    }

    public class GeoLocation
    {
        public string? City { get; set; }
        public string? CountryCode { get; set; }
    }
}