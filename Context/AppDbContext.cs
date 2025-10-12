using Microsoft.EntityFrameworkCore;
using Pic.Tables;

namespace Pic.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Transacao> Transacaos { get; set; }
        public DbSet<Produto> Produto { get; set; }
    }
}
