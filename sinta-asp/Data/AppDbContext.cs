using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;

namespace sinta_asp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Daftarkan semua tabel di sini
        public DbSet<Mahasiswa> Mahasiswa { get; set; }
        public DbSet<Admin> Admins { get; set; }
    }
}