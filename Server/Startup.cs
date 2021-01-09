using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AuthServer.Server.GRPC;
using AuthServer.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hangfire;
using Hangfire.PostgreSql;
using Dapper;
using AuthServer.Server.Services;
using AuthServer.Server.Services.Email;
using AuthServer.Server.GRPC.Security;
using AuthServer.Server.Services.Authentication;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.User;
using AuthServer.Server.GRPC.Admin;
using AuthServer.Server.Services.Ldap;
using AuthServer.Server.Services.Crypto;
using AuthServer.Server.Services.TLS;
using AuthServer.Server.Services.Authentication.Filter;
using AuthServer.Server.Services.Authentication.TwoFactorAuthenticators;
using AuthServer.Server.Services.Crypto.OIDC;
using System.Threading.Tasks;
using AuthServer.Server.Services.ReverseProxy;
using AuthServer.Server.Services.ReverseProxy.Configuration;
using AuthServer.Server.Services.ReverseProxy.Authentication;
using AuthServer.Server.Services.Crypto.JWT;
using AuthServer.Server.Services.SCIM;
using Gatekeeper.Server.Services.FileStorage;
using Gatekeeper.Server.GRPC;
using AuthServer.Server.Services.TLS.BackgroundJob;
using Gatekeeper.Server.Services.Authentication.PasswordPolicy;
using Gatekeeper.Server.Services.GeoLocation;
using Gatekeeper.Server.Services.Authentication.BackgroundJob;
using Gatekeeper.Server.Services.DeviceDetection;

namespace AuthServer.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // DB Context
            services.AddDbContextFactory<AuthDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("AuthDb"),
                    o => o.UseNodaTime()
                )
            );
            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("AuthDb"),
                    o => o.UseNodaTime()
                )
            );

            // Keystorage DB
            services.AddDbContext<KeyStorageDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("KeyStorageDb"),
                    o => o.UseNodaTime()
                )
            );
            services.AddDataProtection()
                .SetApplicationName("Gatekeeper Server")
                .PersistKeysToDbContext<KeyStorageDbContext>();

            // Identity
            services.AddScoped<TotpAuthenticatorProvider>();
            services.AddScoped<CookieAuthenticationEventListener>();
            services.AddIdentity<AppUser, IdentityRole<Guid>>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
                config.Password.RequiredLength = 10;
                config.Password.RequireLowercase = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireDigit = false;
            })
                .AddPasswordValidator<HIBP>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddTokenProvider<TotpAuthenticatorProvider>(TotpAuthenticatorProvider.ProviderName);
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "asid";
                options.LoginPath = "/login";
                options.EventsType = typeof(CookieAuthenticationEventListener);
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdministrator", policy => policy.RequireRole(new string[1] { "admin" }));
            });

            // TLS
            services.AddSingleton<AcmeChallengeSingleton>();
            services.AddScoped<IRequestAcmeCertificateJob, RequestAcmeCertificateJob>();
            services.AddScoped<CertificateRepository>();

            // Framework
            services.AddGrpc();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpClient();

            // Email
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            // User
            services.AddScoped<UserManager>();

            // Authentication
            services.AddScoped<SessionManager>();
            services.AddScoped<BruteforceManager>();

            // Crypto
            services.AddScoped<SecureRandom>();
            services.AddScoped<Hasher>();
            services.AddScoped<OIDCKeyManager>();
            services.AddScoped<JwtFactory>();

            // Configuration
            services.AddScoped<Services.ConfigurationProvider>();

            // LDAP
            services.AddScoped<LdapEventListener>();
            services.AddHostedService<LdapServerListener>();

            // SCIM
            services.AddScoped<ISyncHandler, SyncHandler>();

            // Geolocation
            services.AddScoped<GeoLocationManager>();
            services.AddScoped<ISessionLocationResolver, SessionLocationResolver>();

            // Device Detection
            services.AddScoped<DeviceDetectionManager>();
            services.AddScoped<ISessionDeviceInfoResolver, SessionDeviceInfoResolver>();

            // File storage
            services.AddScoped<ProfileImageManager>();

            // HIBP
            services.AddScoped<HIBPClient>();

            // Hangfire
            SqlMapper.AddTypeHandler(new NodaDateTimeHandler());
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(Configuration.GetConnectionString("HangfireDb")));

            // Miniprofiler
            services.AddMiniProfiler(options =>
            {
                options.ResultsAuthorize = request => MiniProfileFilter.CanSeeProfiler(request);
                options.ResultsListAuthorize = request => MiniProfileFilter.CanSeeProfiler(request);
            }).AddEntityFramework();

            // Reverse Proxy
            services.AddSingleton<ProxyHttpClientProvider>();
            services.AddSingleton<MemorySingletonProxyConfigProvider>();
            services.AddScoped<MemoryPopulator>();
            services.AddScoped<AuthenticationManager>();
            services.AddScoped<SingleSignOnHandler>();
            services.AddHttpProxy();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            UpdateDatabase(app);

            // Reverse Proxy
            app.UseMiddleware<ReverseProxyMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            // Routing
            app.UseRouting();

            // GRPC
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

            // Auth
            app.UseAuthentication();
            app.UseAuthorization();

            // MiniProfiler
            app.UseMiniProfiler();

            // Hangfire
            app.UseHangfireServer();
            var options = new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangfireAuthorizationFilter(),
                }
            };
            app.UseHangfireDashboard("/hangfire", options);

            // Endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<AuthService>();
                endpoints.MapGrpcService<TypeaheadService>();
                endpoints.MapGrpcService<SessionsService>();
                endpoints.MapGrpcService<SettingsService>();
                endpoints.MapGrpcService<UsersService>();
                endpoints.MapGrpcService<GRPC.Admin.AppsService>();
                endpoints.MapGrpcService<GroupsService>();
                endpoints.MapGrpcService<GRPC.Apps.AppsService>();
                endpoints.MapGrpcService<InstallService>();
                endpoints.MapGrpcService<OIDCUserService>();
                endpoints.MapGrpcService<UserProfileService>();
                endpoints.MapGrpcService<ConnectivityServiceCheck>();
                endpoints.MapGrpcService<SsoTokenService>();
                endpoints.MapGrpcService<LetsEncryptService>();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var authDbContext = serviceScope.ServiceProvider.GetService<AuthDbContext>();

            if (authDbContext == null)
            {
                throw new Exception("AuthDbContext is null");
            }

            authDbContext.Database.Migrate();

            using var keyStorageDbContext = serviceScope.ServiceProvider.GetService<KeyStorageDbContext>();

            if (keyStorageDbContext == null)
            {
                throw new Exception("KeyStorageDbContext is null");
            }

            keyStorageDbContext.Database.Migrate();

            CreateAdminRole(serviceScope.ServiceProvider).GetAwaiter().GetResult();
            PopulateReverseProxyConfig(serviceScope.ServiceProvider).GetAwaiter().GetResult();
        }

        private static async Task PopulateReverseProxyConfig(IServiceProvider serviceProvider)
        {
            MemoryPopulator populator = serviceProvider.GetRequiredService<MemoryPopulator>();
            await populator.PopulateFromDatabase();
        }

        private static async Task CreateAdminRole(IServiceProvider serviceProvider)
        {
            RoleManager<IdentityRole<Guid>> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            UserManager userManager = serviceProvider.GetRequiredService<UserManager>();

            bool adminRoleExists = await roleManager.RoleExistsAsync("admin");
            if (!adminRoleExists)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>("admin"));
            }
        }
    }
}
