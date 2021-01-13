using System;
using System.Data;
using Dapper;

namespace AuthServer.Server.Services
{

    public class NodaDateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override DateTime Parse(object value)
        {
            if (value is NodaTime.Instant instant)
            {
                return instant.ToDateTimeUtc();
            }

            throw new NotImplementedException();
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }
    }
}