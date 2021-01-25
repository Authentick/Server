using System;
using System.Collections.Generic;
using System.Net;

namespace Gatekeeper.Server.Web.Services.Alerts.Types
{
    public class BruteforceIpAddressAlert : ISystemAlert
    {
        public AlertTypeEnum AlertType { get => AlertTypeEnum.BruteforceIpAddressAlert; set => throw new System.NotImplementedException(); }
        public Guid Id { get; set; }
        public AlertLevelEnum AlertLevel { get => AlertLevelEnum.Low; set => throw new NotImplementedException(); }
        public bool IsActionable { get => false; set => throw new NotImplementedException(); }

        public readonly IPAddress IpAddress;
        private const string IP_ADDRESS = "ip";

        public BruteforceIpAddressAlert(IPAddress ipAddress)
        {
            IpAddress = ipAddress;
        }

        public BruteforceIpAddressAlert(Dictionary<string, string> dictionary)
        {
            IpAddress = IPAddress.Parse(dictionary[IP_ADDRESS]);
        }

        public Dictionary<string, string> GetDictionary()
        {
            return new Dictionary<string, string>()
            {
                {
                    IP_ADDRESS,
                    IpAddress.ToString()
                }
            };
        }
    }
}
