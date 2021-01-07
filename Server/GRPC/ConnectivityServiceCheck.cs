using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services;
using AuthServer.Server.Services.User;
using AuthServer.Shared;
using Grpc.Core;

namespace AuthServer.Server.GRPC
{
    public class ConnectivityServiceCheck : AuthServer.Shared.ConnectivityCheckService.ConnectivityCheckServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager _userManager;
        private readonly ConfigurationProvider _configurationProvider;

        public ConnectivityServiceCheck(
            IHttpClientFactory httpClientFactory,
            UserManager userManager,
            ConfigurationProvider configurationProvider
            )
        {
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _configurationProvider = configurationProvider;
        }

        public override async Task<IsPublicAccessibleReply> IsPublicAccessible(IsPublicAccessibleRequest request, ServerCallContext context)
        {
            bool isInstalled = _configurationProvider.TryGet(InstallService.INSTALLED_KEY, out string installedValue);
            if (isInstalled)
            {
                AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);
                bool isInRole = await _userManager.IsInRoleAsync(user, "admin");
                if (!isInRole)
                {
                    throw new Exception("Unauthorized access");
                }
            }

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
                HttpResponseMessage responseMessage = await client.PostAsync("https://connectivity-check-services.gatekeeper.page", content, context.CancellationToken);
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
