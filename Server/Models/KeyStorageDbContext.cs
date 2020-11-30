using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Models
{
    class KeyStorageDbContext : DbContext, IDataProtectionKeyContext
    {
        public KeyStorageDbContext(DbContextOptions<KeyStorageDbContext> options) : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    }
}
