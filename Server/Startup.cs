using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AuthServer.Server.GRPC;
using AuthServer.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hangfire;
using Hangfire.PostgreSql;
using Dapper;
using AuthServer.Server.Services;
using AuthServer.Server.Services.Email;
using AuthServer.Server.GRPC.Security;
using AuthServer.Server.Services.Authentication;

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
            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("AuthDb"),
                    o => o.UseNodaTime()
                )
            );

            // Identity
            services.AddScoped<CookieAuthenticationEventListener>();
            services.AddIdentity<AppUser, IdentityRole<Guid>>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "asid";
                options.LoginPath = "/login";
                options.EventsType = typeof(CookieAuthenticationEventListener);
            });

            // Framework
            services.AddGrpc();
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Email
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            // Hangfire
            SqlMapper.AddTypeHandler(new NodaDateTimeHandler());
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(Configuration.GetConnectionString("HangfireDb")));

            // Reverse Proxy
            services.AddReverseProxy().LoadFromConfig(Configuration.GetSection("ReverseProxy")); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            UpdateDatabase(app);

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

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            // Routing
            app.UseRouting();

            // Hangfire
            app.UseHangfireServer();
            app.UseHangfireDashboard();

            // GRPC
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

            // Auth
            app.UseAuthentication();
            app.UseAuthorization();

            // Endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapGrpcService<AuthService>();
                endpoints.MapGrpcService<SessionsService>();
                endpoints.MapGrpcService<SettingsService>();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var authDbContext = serviceScope.ServiceProvider.GetService<AuthDbContext>();

            if(authDbContext == null) {
                throw new Exception("AuthDbContext is null");
            }

            authDbContext.Database.Migrate();
        }
    }
}
