using System.Collections.Generic;
using System.Threading.Tasks;
using Gatekeeper.Shared.ClientAndWeb.Admin;
using Gatekeeper.Server.Web.Services.Alerts;
using Gatekeeper.Server.Web.Services.Alerts.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using static Gatekeeper.Shared.ClientAndWeb.Admin.AdminAlerts;
using System;

namespace Gatekeeper.Server.Web.GRPC.Admin
{
    [Authorize(Policy = "SuperAdministrator")]
    public class AdminAlertsService : AdminAlertsBase
    {
        private readonly AlertManager _alertManager;

        public AdminAlertsService(AlertManager alertManager)
        {
            _alertManager = alertManager;
        }

        public override async Task<DismissAlertReply> DismissAlert(DismissAlertRequest request, ServerCallContext context)
        {
            bool wasSuccessful = await _alertManager.TryDismissSystemAlertAsync(new Guid(request.Id));

            return new DismissAlertReply
            {
                Success = wasSuccessful,
            };
        }

        public override async Task<AlertListReply> ListAlerts(Empty request, ServerCallContext context)
        {
            List<IAlert> alerts = await _alertManager.GetSystemAlertsAsync();
            AlertListReply reply = new AlertListReply { };

            foreach (IAlert alert in alerts)
            {
                System.Type alertType = alert.GetType();
                Alert replyAlert = new Alert
                {
                    Id = alert.Id.ToString(),
                    Level = AlertProtobufConverter.ConvertEnum(alert.AlertLevel),
                    IsActionable = alert.IsActionable,
                };

                if (alertType == typeof(LdapUnencryptedConnectionAlert))
                {
                    var castedAlert = (LdapUnencryptedConnectionAlert)alert;
                    replyAlert.LdapConnectionAlert = new Alert.Types.UnencryptedLdapConnectionAlert
                    {
                        AppName = castedAlert.LdapAppSettingsId.ToString(),
                        IpAddress = (castedAlert.IpAddress.IsIPv4MappedToIPv6) ? castedAlert.IpAddress.MapToIPv4().ToString() : castedAlert.IpAddress.ToString(),
                    };
                }
                else if (alertType == typeof(BruteforceIpAddressAlert))
                {
                    var castedAlert = (BruteforceIpAddressAlert)alert;
                    replyAlert.BruteforceIpAddressAlert = new Alert.Types.BruteforceIpAddressAlert
                    {
                        IpAddress = (castedAlert.IpAddress.IsIPv4MappedToIPv6) ? castedAlert.IpAddress.MapToIPv4().ToString() : castedAlert.IpAddress.ToString(),
                    };
                }

                reply.Alerts.Add(replyAlert);
            }

            return reply;
        }
    }
}
