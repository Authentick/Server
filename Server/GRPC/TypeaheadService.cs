using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Shared;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC
{
    [Authorize]
    public class TypeaheadService : AuthServer.Shared.Typeahead.TypeaheadBase
    {
        private readonly AuthDbContext _authDbContext;

        public TypeaheadService(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public override async Task<SearchGroupReply> SearchGroup(SearchGroupRequest request, ServerCallContext context)
        {
            List<UserGroup> groups = await _authDbContext.UserGroup
                .AsNoTracking()
                .Where(u => u.Name.Contains(request.SearchParameter))
                .ToListAsync();

            SearchGroupReply reply = new SearchGroupReply();

            foreach (UserGroup group in groups)
            {
                SearchGroupEntry entry = new SearchGroupEntry
                {
                    Id = group.Id.ToString(),
                    Name = group.Name,
                };

                reply.Entries.Add(entry);
            }

            return reply;
        }

        public override async Task<SearchUserReply> SearchUser(SearchUserRequest request, ServerCallContext context)
        {
            List<AppUser> users = await _authDbContext.Users
                .AsNoTracking()
                .Where(u => u.UserName.Contains(request.SearchParameter))
                .ToListAsync();

            SearchUserReply reply = new SearchUserReply();

            foreach (AppUser user in users)
            {
                SearchUserEntry entry = new SearchUserEntry
                {
                    Id = user.Id.ToString(),
                    Name = user.UserName,
                };

                reply.Entries.Add(entry);
            }

            return reply;
        }

        public override string? ToString()
        {
            return base.ToString();
        }
    }
}
