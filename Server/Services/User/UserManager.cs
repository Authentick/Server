using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthServer.Server.Services.User
{
    public class UserManager : UserManager<AppUser>
    {
        public UserManager(
            IUserStore<AppUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<AppUser> passwordHasher,
            IEnumerable<IUserValidator<AppUser>> userValidators,
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<AppUser>> logger
            ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {

        }

        
    }
}