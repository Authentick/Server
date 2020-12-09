using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthServer.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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

                                string? snapFolder = Environment.GetEnvironmentVariable("SNAP_USER_DATA");
                                string targetHostName = stream.TargetHostName.Replace("/", "").Replace("\\", "");
                                string targetFile = snapFolder + "/" + targetHostName + ".pfx";

                                SslServerAuthenticationOptions options = new SslServerAuthenticationOptions { };

                                if (File.Exists(targetFile))
                                {
                                    options.ServerCertificate = X509Certificate.CreateFromCertFile(targetFile);
                                }
                                else
                                {
                                    // Use default certificate
                                }

                                return options;
                            }, state: null);
                        });
                        kestrelOptions.ListenAnyIP(80);
                    });
                });
    }
}
