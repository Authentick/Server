using System;
using Gatekeeper.LdapServerLibrary;

namespace Gatekeeper.Server.Ldap
{
    class ConsoleLogger : ILogger
    {
        public void LogException(Exception e)
        {
            System.Console.WriteLine(e.ToString());
        }
    }
}
