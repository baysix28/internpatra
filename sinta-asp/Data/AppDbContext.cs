using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;

namespace sinta_asp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Ini perintah: "Tolong buatkan tabel bernama 'Pendaftarans' berdasarkan model Pendaftaran"
        public DbSet<Pendaftaran> Pendaftarans { get; set; }
    }
}