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

        // ===== USER / ADMIN (Dari branch vava) =====
        public DbSet<Admin> Admins { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }

        // ===== MAHASISWA =====
        public DbSet<Mahasiswa> Mahasiswa { get; set; }

        // ===== FITUR LOWONGAN (Dari branch vava3) =====
        public DbSet<Lowongan> Lowongan { get; set; }

        // ===== PENDAFTARAN PENELITIAN =====
        public DbSet<Pendaftaran> Pendaftarans { get; set; }

        // ===== MAGANG =====
        public DbSet<Magang> PendaftaranMagang { get; set; }

        // ===== NOTIFIKASI (Penting untuk lonceng) =====
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}