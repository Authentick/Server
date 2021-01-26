using System;
using System.Collections.Generic;

namespace Gatekeeper.Server.Web.Services.Alerts.Types
{
    public interface IAlert
    {
        Guid Id { get; set; }
        AlertTypeEnum AlertType { get; set; }
        AlertLevelEnum AlertLevel { get; set; }
        bool IsActionable { get; set; }
        Dictionary<string, string> GetDictionary();
    }

    public enum AlertLevelEnum
    {
        Low = 1,
        Medium = 2,
        High = 3,
    }

    public enum AlertTypeEnum
    {
        UnencryptedLdapBindAlert = 1,
        BruteforceIpAddressAlert = 2,
        BruteforceUserAlert = 3,
    }
}
