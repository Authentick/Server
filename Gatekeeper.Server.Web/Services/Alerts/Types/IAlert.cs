using System;
using System.Collections.Generic;

namespace Gatekeeper.Server.Web.Services.Alerts.Types
{
    public interface IAlert
    {
        Guid Id { get; set; }
        AlertTypeEnum AlertType { get; set; }
        Dictionary<string, string> GetDictionary();
    }

    public enum AlertTypeEnum
    {
        UnencryptedLdapBindAlert = 1,
    }
}
