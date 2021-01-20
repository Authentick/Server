using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto;
using AuthServer.Server.Services.Ldap;
using Gatekeeper.Server.Services.FileStorage;
using Gatekeeper.Server.Web.Services.Alerts;
using Gatekeeper.Server.Web.Services.Alerts.Types;
using Gatekeeper.Shared.LdapAndWeb;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Gatekeeper.Server.Web.GRPC
{
    public class LdapService : Gatekeeper.Shared.LdapAndWeb.Ldap.LdapBase
    {
        private readonly AuthDbContext _authDbContext;
        private readonly IDataProtector _ldapSettingsDataProtector;
        private readonly Hasher _hasher;
        private readonly AlertManager _alertManager;

        public LdapService(
            AuthDbContext authDbContext,
            IDataProtectionProvider dataProtectionProvider,
            Hasher hasher,
            AlertManager alertManager
            )
        {
            _authDbContext = authDbContext;
            _ldapSettingsDataProtector = dataProtectionProvider.CreateProtector("LdapSettingsDataProtector");
            _hasher = hasher;
            _alertManager = alertManager;
        }

        // TODO: This should actually use some kind of authentication that isn't IP-based
        private bool RequestIsFromLoopback(ServerCallContext context)
        {
            var connection = context.GetHttpContext().Connection;
            if (connection.RemoteIpAddress == null)
            {
                return false;
            }

            return IPAddress.IsLoopback(connection.RemoteIpAddress);
        }

        public override async Task<CertificatePathReply> GetCertificatePath(Empty request, ServerCallContext context)
        {
            if (!RequestIsFromLoopback(context))
            {
                throw new Exception("Request is not from loopback");
            }

            string snapFolder = PathProvider.GetApplicationDataFolder();
            string primaryDomainConfigFile = snapFolder + "/primary-domain.txt";
            string primaryDomain = await File.ReadAllTextAsync(primaryDomainConfigFile);

            return new CertificatePathReply
            {
                Path = snapFolder + "/" + primaryDomain + ".pfx",
            };
        }

        public override async Task<BindReply> ExecuteBindRequest(BindRequest request, ServerCallContext context)
        {
            if (!RequestIsFromLoopback(context))
            {
                throw new Exception("Request is not from loopback");
            }

            List<string> cns = request.UserIdentity.Cn.ToList();
            List<string> dcs = request.UserIdentity.Dc.ToList();
            List<string> ous = request.UserIdentity.Ou.ToList();

            Guid appGuid = new Guid(dcs[0]);
            LdapAppSettings? settings = await _authDbContext.LdapAppSettings
                .Include(s => s.AuthApp)
                .SingleOrDefaultAsync(s => s.AuthApp.Id == appGuid);

            if (settings == null)
            {
                return new BindReply
                {
                    WasBindSuccessful = false
                };
            }

            if (!request.IsEncryptedRequest)
            {
                LdapUnencryptedConnectionAlert alert = new LdapUnencryptedConnectionAlert
                (
                    IPAddress.Parse(request.IpAddress),
                    settings
                );
                await _alertManager.AddAlertAsync(alert);
            }

            if (cns.Count > 0 && dcs.Count > 0)
            {
                if (cns[0] == "BindUser" && ous.Count == 0)
                {
                    if (settings != null)
                    {
                        byte[] correctPassword = Encoding.ASCII.GetBytes(_ldapSettingsDataProtector.Unprotect(settings.BindUserPassword));
                        byte[] providedPassword = Encoding.ASCII.GetBytes(request.Password);
                        bool isCorrectPassword = CryptographicOperations.FixedTimeEquals(correctPassword, providedPassword);

                        return new BindReply
                        {
                            WasBindSuccessful = isCorrectPassword
                        };
                    }
                }
                else if (ous.Count > 0 && ous[0] == "people")
                {
                    Guid userId = new Guid(cns[0]);
                    IEnumerable<LdapAppUserCredentials> creds = await _authDbContext.LdapAppUserCredentials
                        .Where(c => c.User.Id == userId)
                        .Where(c => c.LdapAppSettings.AuthApp.Id == appGuid)
                        .ToListAsync();

                    bool validCredentials = false;

                    CancellationTokenSource cts = new CancellationTokenSource();
                    ParallelOptions po = new ParallelOptions();
                    po.CancellationToken = cts.Token;
                    po.CancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        Parallel.ForEach(creds, po, (cred) =>
                        {
                            bool isValid = _hasher.VerifyHash(cred.HashedPassword, request.Password);
                            if (isValid)
                            {
                                validCredentials = true;
                                cts.Cancel();
                            }
                        });
                    }
                    catch (OperationCanceledException) { }
                    finally
                    {
                        cts.Dispose();
                    }

                    return new BindReply
                    {
                        WasBindSuccessful = validCredentials
                    };
                }
            }

            return new BindReply
            {
                WasBindSuccessful = false
            };
        }

        public override async Task<SearchReply> ExecuteSearchRequest(SearchRequest request, ServerCallContext context)
        {
            if (!RequestIsFromLoopback(context))
            {
                throw new Exception("Request is not from loopback");
            }

            SearchReply reply = new SearchReply { };

            Guid appId = new Guid(request.UserIdentity.Dc[0]);
            LdapPacketParserLibrary.Parser parser = new LdapPacketParserLibrary.Parser();
            LdapPacketParserLibrary.Models.LdapMessage message = parser.TryParsePacket(request.RawPacket.ToByteArray());

            if (message.ProtocolOp.GetType() == typeof(LdapPacketParserLibrary.Models.Operations.Request.SearchRequest))
            {
                LdapPacketParserLibrary.Models.Operations.Request.SearchRequest searchRequest =
                    (LdapPacketParserLibrary.Models.Operations.Request.SearchRequest)message.ProtocolOp;


                int? limit = searchRequest.SizeLimit;

                var itemExpression = Expression.Parameter(typeof(AppUser));
                SearchExpressionBuilder searchExpressionBuilder = new SearchExpressionBuilder();
                var conditions = searchExpressionBuilder.Build(searchRequest.Filter, itemExpression);
                var queryLambda = Expression.Lambda<Func<AppUser, bool>>(conditions, itemExpression);
                var predicate = queryLambda.Compile();

                List<AppUser> results = await _authDbContext.Users
                        .AsNoTracking()
                        .Include(u => u.Groups)
                            .ThenInclude(g => g.AuthApps)
                        .Where(queryLambda)
                        .Where(u => u.Groups.Any(g => g.AuthApps.Any(a => a.Id == appId)))
                        .AsSplitQuery()
                        .ToListAsync();

                SearchReply.Types.ResultEntry entry = new SearchReply.Types.ResultEntry { };

                foreach (AppUser user in results)
                {
                    entry.Rdn = "cn=" + user.Id.ToString() + ",ou=People,dc=" + appId;

                    SearchReply.Types.ResultEntry.Types.ResultAttribute displayNameAttribute = new SearchReply.Types.ResultEntry.Types.ResultAttribute
                    {
                        Name = "displayname",
                    };
                    displayNameAttribute.Value.Add(user.UserName);

                    SearchReply.Types.ResultEntry.Types.ResultAttribute emailAttribute = new SearchReply.Types.ResultEntry.Types.ResultAttribute
                    {
                        Name = "email",
                    };
                    emailAttribute.Value.Add(user.Email);

                    SearchReply.Types.ResultEntry.Types.ResultAttribute objectClassAttribute = new SearchReply.Types.ResultEntry.Types.ResultAttribute
                    {
                        Name = "objectclass",
                    };
                    objectClassAttribute.Value.Add("inetOrgPerson");

                    SearchReply.Types.ResultEntry.Types.ResultAttribute entryUuidAttribute = new SearchReply.Types.ResultEntry.Types.ResultAttribute
                    {
                        Name = "entryUuid",
                    };
                    entryUuidAttribute.Value.Add(user.Id.ToString());

                    entry.Attributes.AddRange(new List<SearchReply.Types.ResultEntry.Types.ResultAttribute>(){
                        displayNameAttribute, emailAttribute, objectClassAttribute, entryUuidAttribute
                    });

                    reply.Results.Add(entry);
                }
            }

            return reply;
        }
    }
}
