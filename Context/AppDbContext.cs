using Microsoft.EntityFrameworkCore;

namespace Pic.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Tables.Usuario> Usuarios { get; set; }
    }
}
