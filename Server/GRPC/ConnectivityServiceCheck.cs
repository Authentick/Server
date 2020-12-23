using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AuthServer.Shared;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Server.GRPC
{
    [Authorize(Policy = "SuperAdministrator")]
    public class ConnectivityServiceCheck : AuthServer.Shared.ConnectivityCheckService.ConnectivityCheckServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConnectivityServiceCheck(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task<IsPublicAccessibleReply> IsPublicAccessible(IsPublicAccessibleRequest request, ServerCallContext context)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            Guid challenge = Guid.NewGuid();
            ConnectivityCheckRequest connectivityCheckRequest = new ConnectivityCheckRequest
            {
                Hostname = request.Hostname,
                Challenge = challenge.ToString(),
            };

            StringContent content = new StringContent(JsonSerializer.Serialize<ConnectivityCheckRequest>(connectivityCheckRequest));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                HttpResponseMessage responseMessage = await client.PostAsync("https://connectivity-check-services.gatekeeper.page", content);
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                ConnectivityCheckResponse response = JsonSerializer.Deserialize<ConnectivityCheckResponse>(responseBody);

                return new IsPublicAccessibleReply
                {
                    State = (response.Success ? IsPublicAccessibleReply.Types.AccessibleReplyEnum.Success : IsPublicAccessibleReply.Types.AccessibleReplyEnum.Failure),
                };
            }
            catch { }

            return new IsPublicAccessibleReply
            {
                State = IsPublicAccessibleReply.Types.AccessibleReplyEnum.Unknown
            };
        }

        private class ConnectivityCheckRequest
        {
            [JsonPropertyName("hostname")]
            public string? Hostname { get; set; }

            [JsonPropertyName("challenge")]
            public string? Challenge { get; set; }
        }

        private class ConnectivityCheckResponse
        {

            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }
    }
}
