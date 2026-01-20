using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;

namespace sinta_asp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tabel Pendaftarans
        public DbSet<Pendaftaran> Pendaftarans { get; set; }

        // Tabel PendaftaranMagang
        public DbSet<Magang> PendaftaranMagang { get; set; }
    }
}
