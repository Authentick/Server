using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Gatekeeper.Server.Web.Services.Alerts.Types;
using Microsoft.EntityFrameworkCore;

namespace Gatekeeper.Server.Web.Services.Alerts
{
    public class AlertManager
    {
        private readonly AuthDbContext _authDbContext;

        public AlertManager(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public async Task<bool> TryDismissAlertAsync(Guid alertId)
        {
            SystemSecurityAlert? alert = await _authDbContext.SystemSecurityAlerts
                .SingleOrDefaultAsync(s => s.Id == alertId);
            if (alert == null)
            {
                return false;
            }

            _authDbContext.Remove(alert);
            await _authDbContext.SaveChangesAsync();

            return true;
        }

        public async Task AddAlertAsync(IAlert alert)
        {
            _authDbContext.SystemSecurityAlerts.Add(
                new SystemSecurityAlert
                {
                    AlertType = alert.AlertType,
                    KeyValueStore = alert.GetDictionary(),
                }
            );

            await _authDbContext.SaveChangesAsync();
        }

        public async Task<List<IAlert>> GetAlertsAsync()
        {
            List<SystemSecurityAlert> alerts = await _authDbContext.SystemSecurityAlerts.ToListAsync();
            List<IAlert> alertList = new List<IAlert>();

            foreach (SystemSecurityAlert alert in alerts)
            {
                if (alert.AlertType == AlertTypeEnum.UnencryptedLdapBindAlert)
                {
                    LdapUnencryptedConnectionAlert ldapUnencryptedConnectionAlert = new LdapUnencryptedConnectionAlert(alert.KeyValueStore);
                    ldapUnencryptedConnectionAlert.Id = alert.Id;
                    alertList.Add(ldapUnencryptedConnectionAlert);
                }
            }

            return alertList;
        }
    }
}
