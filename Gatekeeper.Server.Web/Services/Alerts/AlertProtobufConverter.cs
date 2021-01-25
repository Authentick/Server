using System.IO;
using Gatekeeper.Server.Web.Services.Alerts.Types;
using static Gatekeeper.Shared.ClientAndWeb.Admin.Alert.Types;

namespace Gatekeeper.Server.Web.Services.Alerts
{
    public static class AlertProtobufConverter
    {
        public static LevelEnum ConvertEnum(AlertLevelEnum alertLevelEnum)
        {
            switch (alertLevelEnum)
            {
                case AlertLevelEnum.High:
                    return LevelEnum.High;
                case AlertLevelEnum.Medium:
                    return LevelEnum.Medium;
                case AlertLevelEnum.Low:
                    return LevelEnum.Low;
                default:
                    throw new InvalidDataException(alertLevelEnum + " is invalid");
            }
        }
    }
}