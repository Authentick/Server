using System.Threading;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthServer.Server.Services.Ldap
{
    class LdapServerListener : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public LdapServerListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                LdapServer server = new LdapServer
                {
                    Port = 389,
                };

                LdapEventListener ldapEventListener = scope.ServiceProvider.GetRequiredService<LdapEventListener>();
                server.RegisterEventListener(ldapEventListener);
                await server.Start();
            }
        }
    }
}
