using System.Net;
using System.Net.Http;

namespace AuthServer.Server.Services.ReverseProxy
{
    class ProxyHttpClientProvider
    {
        private readonly HttpMessageInvoker _client;

        public ProxyHttpClientProvider()
        {
            _client = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });
        }

        public HttpMessageInvoker GetClient()
        {
            return _client;
        }
    }
}
