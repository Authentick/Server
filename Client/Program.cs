using AuthServer.Client.Util;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace AuthServer.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddScoped(services => { return new AuthServer.Shared.Typeahead.TypeaheadClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Auth.AuthClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Security.Settings.SettingsClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Security.Sessions.SessionsClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Admin.Users.UsersClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Admin.AdminApps.AdminAppsClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Admin.Groups.GroupsClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Apps.Apps.AppsClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.Install.InstallClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.UserProfile.UserProfileClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.OIDCUserService.OIDCUserServiceClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.ConnectivityCheckService.ConnectivityCheckServiceClient(GetGrpcChannel(services)); });
            builder.Services.AddScoped(services => { return new AuthServer.Shared.SsoTokenService.SsoTokenServiceClient(GetGrpcChannel(services)); });

            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
            builder.Services.AddScoped<InstallationStateProvider>();
            builder.Services.AddScoped<MobileNavigationStateProvider>();

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("SuperAdministrator", policy => policy.RequireRole(new string[1] { "admin" }));
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
