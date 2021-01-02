using System;
using System.Threading;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthServer.Server.Services.Ldap
{
    class LdapServerListener : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<LdapServerListener> _logger;

        public LdapServerListener(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<LdapServerListener> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                LdapServer server = new LdapServer
                {
                    Port = 3389,
                };

                LdapEventListener ldapEventListener = scope.ServiceProvider.GetRequiredService<LdapEventListener>();
                server.RegisterEventListener(ldapEventListener);
                server.RegisterLogger(new LdapLogger(_logger));
                await server.Start();
            }
        }

        private class LdapLogger : Gatekeeper.LdapServerLibrary.ILogger
        {
            private readonly ILogger<LdapServerListener> _logger;

            public LdapLogger(ILogger<LdapServerListener> logger)
            {
                _logger = logger;
            }

            public void LogException(Exception e)
            {
                _logger.LogError(1, e, "Parsing LDAP packet failed");
            }
        }
    }
}
