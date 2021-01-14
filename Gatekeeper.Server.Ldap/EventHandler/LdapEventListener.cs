using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary;
using Gatekeeper.LdapServerLibrary.Session.Events;
using Gatekeeper.LdapServerLibrary.Session.Replies;
using Gatekeeper.Shared.LdapAndWeb;
using static Gatekeeper.Shared.LdapAndWeb.Ldap;

namespace Gatekeeper.Server.Ldap.EventHandler
{
    public class LdapEventListener : LdapEvents
    {
        private readonly LdapClient _ldapClient;

        public LdapEventListener(LdapClient ldapClient)
        {
            _ldapClient = ldapClient;
        }

        private UserIdentity ExtractUserIdentity(Dictionary<string, List<string>> rdn)
        {
            List<string>? cns;
            rdn.TryGetValue("cn", out cns);

            List<string>? dcs;
            rdn.TryGetValue("dc", out dcs);

            List<string>? ous;
            rdn.TryGetValue("ou", out ous);

            UserIdentity identity = new UserIdentity();

            if (cns != null)
            {
                identity.Cn.AddRange(cns);
            }

            if (dcs != null)
            {
                identity.Dc.AddRange(dcs);
            }

            if (ous != null)
            {
                identity.Ou.AddRange(ous);
            }

            return identity;
        }

        public override async Task<bool> OnAuthenticationRequest(ClientContext context, IAuthenticationEvent authenticationEvent)
        {
            BindRequest bindRequest = new BindRequest
            {
                UserIdentity = ExtractUserIdentity(authenticationEvent.Rdn),
                IsEncryptedRequest = context.HasEncryptedConnection,
                Password = authenticationEvent.Password,
                IpAddress = context.IpAddress.ToString(),
            };

            var response = await _ldapClient.ExecuteBindRequestAsync(bindRequest);
            return response.WasBindSuccessful;
        }

        public override async Task<List<SearchResultReply>> OnSearchRequest(ClientContext context, ISearchEvent searchEvent)
        {
            SearchRequest searchRequest = new SearchRequest
            {
                RawPacket = Google.Protobuf.ByteString.CopyFrom(searchEvent.SearchRequest.RawPacket),
                UserIdentity = ExtractUserIdentity(context.Rdn),
            };

            var response = await _ldapClient.ExecuteSearchRequestAsync(searchRequest);

            List<SearchResultReply> replies = new List<SearchResultReply>();
            foreach (var result in response.Results)
            {
                List<SearchResultReply.Attribute> attributes = new List<SearchResultReply.Attribute>();

                foreach (var attribute in result.Attributes)
                {
                    SearchResultReply.Attribute ldapAttribute = new SearchResultReply.Attribute(
                        attribute.Name,
                        attribute.Value.ToList()
                    );
                    attributes.Add(ldapAttribute);
                }

                SearchResultReply reply = new SearchResultReply(result.Rdn, attributes);
                replies.Add(reply);
            }

            return replies;
        }
    }
}
