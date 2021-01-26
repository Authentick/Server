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

        public async Task<bool> TryDismissUserAlertAsync(AppUser user, Guid alertId)
        {
            UserSecurityAlert? alert = await _authDbContext.UserSecurityAlerts
                .Where(a => a.Recipient == user)
                .SingleOrDefaultAsync(s => s.Id == alertId);
            if (alert == null)
            {
                return false;
            }

            _authDbContext.Remove(alert);
            await _authDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TryDismissSystemAlertAsync(Guid alertId)
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
            if (alert is ISystemAlert)
            {
                _authDbContext.SystemSecurityAlerts.Add(
                    new SystemSecurityAlert
                    {
                        AlertType = alert.AlertType,
                        KeyValueStore = alert.GetDictionary(),
                    }
                );
            }

            if (alert is IUserAlert)
            {
                _authDbContext.UserSecurityAlerts.Add(
                    new UserSecurityAlert
                    {
                        AlertType = alert.AlertType,
                        KeyValueStore = alert.GetDictionary(),
                        Recipient = ((IUserAlert)alert).TargetUser,
                    }
                );
            }

            await _authDbContext.SaveChangesAsync();
        }

        public async Task<List<IAlert>> GetUserAlertsAsync(AppUser user)
        {
            List<UserSecurityAlert> alerts = await _authDbContext.UserSecurityAlerts.ToListAsync();
            List<IAlert> alertList = new List<IAlert>();

            foreach (UserSecurityAlert alert in alerts)
            {
                switch (alert.AlertType)
                {
                    case AlertTypeEnum.BruteforceUserAlert:
                        BruteforceUserAlert bruteforceUserAlert = new BruteforceUserAlert(user);
                        bruteforceUserAlert.Id = alert.Id;
                        alertList.Add(bruteforceUserAlert);
                        break;
                }
            }

            return alertList;
        }

        public async Task<List<IAlert>> GetSystemAlertsAsync()
        {
            List<SystemSecurityAlert> alerts = await _authDbContext.SystemSecurityAlerts.ToListAsync();
            List<IAlert> alertList = new List<IAlert>();

            foreach (SystemSecurityAlert alert in alerts)
            {
                switch (alert.AlertType)
                {
                    case AlertTypeEnum.UnencryptedLdapBindAlert:
                        LdapUnencryptedConnectionAlert ldapUnencryptedConnectionAlert = new LdapUnencryptedConnectionAlert(alert.KeyValueStore);
                        ldapUnencryptedConnectionAlert.Id = alert.Id;
                        alertList.Add(ldapUnencryptedConnectionAlert);
                        break;
                    case AlertTypeEnum.BruteforceIpAddressAlert:
                        BruteforceIpAddressAlert bruteforceIpAddressAlert = new BruteforceIpAddressAlert(alert.KeyValueStore);
                        bruteforceIpAddressAlert.Id = alert.Id;
                        alertList.Add(bruteforceIpAddressAlert);
                        break;
                }
            }

            return alertList;
        }
    }
}
