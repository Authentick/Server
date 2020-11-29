using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Gatekeeper.LdapServerLibrary;
using Gatekeeper.LdapServerLibrary.Session.Events;
using Gatekeeper.LdapServerLibrary.Session.Replies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Services.Ldap
{
    public class LdapEventListener : LdapEvents
    {
        private readonly AuthDbContext _authDbContext;
        private readonly IDataProtector _ldapSettingsDataProtector;

        public LdapEventListener(
            AuthDbContext authDbContext,
            IDataProtectionProvider dataProtectionProvider
            )
        {
            _authDbContext = authDbContext;
            _ldapSettingsDataProtector = dataProtectionProvider.CreateProtector("LdapSettingsDataProtector");
        }

        public override async Task<bool> OnAuthenticationRequest(ClientContext context, AuthenticationEvent authenticationEvent)
        {
            string[] splittedUsername = authenticationEvent.Username.Split(",");
            string dc = "";
            string ou = "";
            string cn = "";

            foreach (string splitString in splittedUsername)
            {
                if (splitString.StartsWith("cn="))
                {
                    cn = splitString;
                }
                else if (splitString.StartsWith("dc="))
                {
                    dc = splitString;
                }
                else if (splitString.StartsWith("ou="))
                {
                    ou = splitString;
                }
            }

            if (cn == "cn=BindUser" && ou == "")
            {
                LdapAppSettings? settings = await _authDbContext.LdapAppSettings
                    .Include(s => s.AuthApp)
                    .SingleOrDefaultAsync(s => s.BindUser == authenticationEvent.Username);

                if(settings != null) {
                    byte[] correctPassword = Encoding.ASCII.GetBytes(_ldapSettingsDataProtector.Unprotect(settings.BindUserPassword));
                    byte[] providedPassword = Encoding.ASCII.GetBytes(authenticationEvent.Password);
                    bool isCorrectPassword = CryptographicOperations.FixedTimeEquals(correctPassword, providedPassword);
                
                    return isCorrectPassword;
                }
            }
            else if (ou == "People")
            {

            }

            return false;
        }

        public override Task<List<SearchResultReply>> OnSearchRequest(ClientContext context, SearchEvent searchEvent)
        {
            int? limit = searchEvent.SizeLimit;

            var itemExpression = Expression.Parameter(typeof(AppUser));
            SearchExpressionBuilder searchExpressionBuilder = new SearchExpressionBuilder(searchEvent);
            var conditions = searchExpressionBuilder.Build(searchEvent.Filter, itemExpression);
            var queryLambda = Expression.Lambda<Func<AppUser, bool>>(conditions, itemExpression);
            var predicate = queryLambda.Compile();

            var results = _authDbContext.Users.Where(predicate).ToList();

            List<SearchResultReply> replies = new List<SearchResultReply>();
            foreach (AppUser user in results)
            {
                List<SearchResultReply.Attribute> attributes = new List<SearchResultReply.Attribute>{
                    new SearchResultReply.Attribute("displayname", new List<string>{user.UserName}),
                    new SearchResultReply.Attribute("email", new List<string>{user.Email}),
                    new SearchResultReply.Attribute("object", new List<string>{"inetOrgPerson"}),
                    new SearchResultReply.Attribute("entryuuid", new List<string>{user.Id.ToString()}),
                };
                SearchResultReply reply = new SearchResultReply(
                    user.Id.ToString(),
                    attributes
                );

                replies.Add(reply);
            }

            return Task.FromResult(replies);
        }
    }
}
