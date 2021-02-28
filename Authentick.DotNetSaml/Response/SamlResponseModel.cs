using System;

namespace Authentick.DotNetSaml.Response
{
    public class SamlResponseModel
    {
        internal readonly string Issuer;
        internal readonly string InResponseTo;
        internal readonly string Audience;
        internal readonly string NameId;
        internal readonly DateTime TimeStamp;
        internal readonly TimeSpan ValidityTimeSpan;
        internal readonly string Destination;

        public SamlResponseModel(
            string issuer,
            string inResponseTo,
            string nameId,
            string audience,
            string destination,
            DateTime timeStamp,
            TimeSpan validityTimeSpan)
        {
            Issuer = issuer;
            InResponseTo = inResponseTo;
            Audience = audience;
            NameId = nameId;
            Destination = destination;
            TimeStamp = timeStamp;
            ValidityTimeSpan = validityTimeSpan;
        }
    }
}
