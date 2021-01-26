using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using Gatekeeper.Server.Web.Services.Alerts;
using Gatekeeper.Server.Web.Services.Alerts.Types;
using Gatekeeper.Shared.ClientAndWeb.Security;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using static Gatekeeper.Shared.ClientAndWeb.Security.SecurityAlerts;

namespace Gatekeeper.Server.Web.GRPC.Security
{
    [Authorize]
    class SecurityAlertsService : SecurityAlertsBase
    {
        private readonly AlertManager _alertManager;
        private readonly UserManager _userManager;

        public SecurityAlertsService(
            AlertManager alertManager,
            UserManager userManager
            )
        {
            _alertManager = alertManager;
            _userManager = userManager;
        }

        public override async Task<DismissAlertReply> DismissAlert(DismissAlertRequest request, ServerCallContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);
            bool success = await _alertManager.TryDismissUserAlertAsync(user, new Guid(request.Id));

            return new DismissAlertReply
            {
                Success = success,
            };
        }

        public override async Task<AlertListReply> ListAlerts(Empty request, ServerCallContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);
            List<IAlert> alerts = await _alertManager.GetUserAlertsAsync(user);

            AlertListReply alertListReply = new AlertListReply { };
            foreach (IAlert alert in alerts)
            {
                System.Type alertType = alert.GetType();

                if (alertType == typeof(BruteforceUserAlert))
                {
                    BruteforceUserAlert castedAlert = (BruteforceUserAlert)alert;

                    alertListReply.Alerts.Add(
                        new Alert
                        {
                            BruteforceUserAlert = new Alert.Types.BruteforceUserAlert(),
                            Level = Shared.ClientAndWeb.Security.Alert.Types.LevelEnum.Low,
                            Id = alert.Id.ToString(),
                        }
                    );
                }

            }

            return alertListReply;
        }
    }
}
