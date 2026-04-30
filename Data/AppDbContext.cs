using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;

namespace sinta_asp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Pendaftaran Penelitian 
        public DbSet<Pendaftaran> Pendaftarans { get; set; }
        public DbSet<Lowongan> Lowongan { get; set; }
    }
}