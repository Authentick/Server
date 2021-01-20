using System;
using System.Collections.Generic;
using System.Net;
using AuthServer.Server.Models;

namespace Gatekeeper.Server.Web.Services.Alerts.Types
{
    public class LdapUnencryptedConnectionAlert : IAlert
    {
        public AlertTypeEnum AlertType { get => AlertTypeEnum.UnencryptedLdapBindAlert; set => throw new System.NotImplementedException(); }
        public Guid Id { get; set; }

        public readonly IPAddress IpAddress;
        public readonly Guid LdapAppSettingsId;

        private const string IP_ADDRESS = "ip";
        private const string LDAP_APPSETTINGS_ID = "laid";

        public LdapUnencryptedConnectionAlert(IPAddress ipAddress, LdapAppSettings ldapAppSettings)
        {
            IpAddress = ipAddress;
            LdapAppSettingsId = ldapAppSettings.Id;
        }

        public LdapUnencryptedConnectionAlert(Dictionary<string, string> dictionary)
        {
            IpAddress = IPAddress.Parse(dictionary[IP_ADDRESS]);
            LdapAppSettingsId = new Guid(dictionary[LDAP_APPSETTINGS_ID]);
        }

        public Dictionary<string, string> GetDictionary()
        {
            return new Dictionary<string, string>()
            {
                {
                    IP_ADDRESS,
                    IpAddress.ToString()
                },
                {
                    LDAP_APPSETTINGS_ID,
                    LdapAppSettingsId.ToString()
                },
            };
        }
    }
}
