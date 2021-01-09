using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace Gatekeeper.Server.Services.DeviceDetection
{
    public class DeviceDetectionManager
    {
        private readonly IHttpClientFactory _clientFactory;

        public DeviceDetectionManager(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<DeviceInfo?> TryResolveLocationAsync(string userAgent)
        {
            HttpClient client = _clientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.DefaultRequestHeaders.Add("User-Agent", "Gatekeeper");

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = "devicedetector-services.gatekeeper.page";
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["userAgent"] = userAgent;
            uriBuilder.Query = query.ToString();

            Uri uri = uriBuilder.Uri;

            try
            {
                DeviceInfo reply = await client.GetFromJsonAsync<DeviceInfo>(uri);
                return reply;
            }
            catch { }

            return null;
        }
    }

    public class DeviceInfo
    {
        [JsonPropertyName("isSmartphone")]
        public bool IsSmartphone { get; set; }
        [JsonPropertyName("isTablet")]
        public bool IsTablet { get; set; }
        [JsonPropertyName("isDesktop")]
        public bool IsDesktop { get; set; }
        [JsonPropertyName("os")]
        public string? OperatingSystemName { get; set; }
        [JsonPropertyName("browser")]
        public string? BrowserName { get; set; }
        [JsonPropertyName("brand")]
        public string? BrandName { get; set; }
        [JsonPropertyName("model")]
        public string? ModelName { get; set; }
    }
}
