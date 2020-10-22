using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;

namespace AuthServer.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped(services =>
            {
                return new AuthServer.Shared.Auth.AuthClient(GetGrpcChannel(services));
            });

            await builder.Build().RunAsync();
        }

        private static GrpcChannel GetGrpcChannel(IServiceProvider services)
        {
            var httpClient = new System.Net.Http.HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new System.Net.Http.HttpClientHandler()));
            var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
            return GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
        }
    }
}
