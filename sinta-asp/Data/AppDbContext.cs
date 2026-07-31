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

        // ===== USER / ADMIN =====
        public DbSet<Mahasiswa> Mahasiswa { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // ===== USER =====
        public DbSet<User> Users { get; set; }

        // ===== PENDAFTARAN PENELITIAN =====
        public DbSet<Pendaftaran> Pendaftarans { get; set; }

        // ===== MAGANG =====
        public DbSet<Magang> PendaftaranMagang { get; set; }

        // ===== LOWONGAN (TAMBAHKAN INI) =====
        public DbSet<Lowongan> Lowongan { get; set; }

        // ===== UserProfile ========
        public DbSet<UserProfile> UserProfile { get; set; }

    }
}
