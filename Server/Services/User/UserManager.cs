using System;
using System.Collections.Generic;
using System.Linq;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthServer.Server.Services.User
{
    public class UserManager : UserManager<AppUser>
    {
        private readonly AuthDbContext _authDbContext;

        public UserManager(
            IUserStore<AppUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<AppUser> passwordHasher,
            IEnumerable<IUserValidator<AppUser>> userValidators,
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<AppUser>> logger,
            AuthDbContext authDbContext
            ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _authDbContext = authDbContext;
        }

        public virtual IEnumerable<AppUser> GetAllUsers()
        {
            List<AppUser> users = _authDbContext.Users.ToList();

            return users;
        }
    }
}