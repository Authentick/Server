using System;
using System.Collections.Generic;
using AuthServer.Server.Models;

namespace Gatekeeper.Server.Web.Services.Alerts.Types
{
    public class BruteforceUserAlert : IUserAlert
    {
        public AlertTypeEnum AlertType { get => AlertTypeEnum.BruteforceUserAlert; set => throw new System.NotImplementedException(); }
        public Guid Id { get; set; }
        public AlertLevelEnum AlertLevel { get => AlertLevelEnum.Low; set => throw new NotImplementedException(); }
        public bool IsActionable { get => false; set => throw new NotImplementedException(); }
        public AppUser TargetUser { get; set; }

        public BruteforceUserAlert(AppUser user)
        {
            TargetUser = user;
        }

        public Dictionary<string, string> GetDictionary()
        {
            return new Dictionary<string, string>();
        }
    }
}
