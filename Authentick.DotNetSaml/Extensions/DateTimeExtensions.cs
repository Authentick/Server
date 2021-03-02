using System;

namespace Authentick.DotNetSaml.Extensions {
    internal static class DateTimeExtensions {
        internal static string ToSamlInstant(this DateTime dateTime) {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
