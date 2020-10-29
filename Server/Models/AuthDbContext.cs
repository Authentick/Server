using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.Models
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AuthSession> AuthSessions { get; set; }
    }

    public class AuthSession
    {
        public int Id { get; set; }
        public Instant CreationTime { get; set; }
    }
}
