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

        // ===== USER / ADMIN (Dari backup_master) =====
        public DbSet<Mahasiswa> Mahasiswa { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // ===== PENDAFTARAN & LOWONGAN (Gabungan) =====
        public DbSet<Pendaftaran> Pendaftarans { get; set; }
        public DbSet<Lowongan> Lowongan { get; set; } // Ini tabel baru dari branch kamu

        // ===== MAGANG (Dari backup_master) =====
        public DbSet<Magang> PendaftaranMagang { get; set; }
    }
}