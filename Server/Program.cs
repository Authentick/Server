using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AuthServer.Server.Services.TLS;
using Gatekeeper.Server.Services.FileStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace AuthServer.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string logFileLocation = PathProvider.GetApplicationDataFolder() + "/gatekeeper-logs.txt";

            NLog.Config.LoggingConfiguration config = new NLog.Config.LoggingConfiguration();
            NLog.Targets.FileTarget logFileTarget = new NLog.Targets.FileTarget("logfile") { FileName = logFileLocation };
            NLog.Targets.ConsoleTarget logConsoleTarget = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsoleTarget);
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, logFileTarget);

            NLog.Logger logger = NLogBuilder.ConfigureNLog(config).GetCurrentClassLogger();

            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel((builderContext, kestrelOptions) =>
                    {
                        kestrelOptions.AddServerHeader = false;

                        kestrelOptions.ListenAnyIP(443, listenOptions =>
                           {
                               listenOptions.UseHttps(async (stream, clientHelloInfo, state, cancellationToken) =>
                               {
                                   await Task.Yield();

                                   SslServerAuthenticationOptions options = new SslServerAuthenticationOptions { };

                                   if (CertificateLocationHelper.CertificateExists(clientHelloInfo.ServerName))
                                   {
                                       options.ServerCertificate = new X509Certificate2(CertificateLocationHelper.GetPath(clientHelloInfo.ServerName));
                                   }
                                   else
                                   {
                                    // FIXME: Use default certificate
                                }

                                   return options;
                               }, state: null);
                           });
                        kestrelOptions.ListenAnyIP(80);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();
    }
}
