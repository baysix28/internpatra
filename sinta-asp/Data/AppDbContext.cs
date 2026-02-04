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
        public DbSet<Admin> Admins { get; set; }

        // ===== USER =====
        public DbSet<User> Users { get; set; }

        // ===== PENDAFTARAN PENELITIAN =====
        public DbSet<Pendaftaran> Pendaftarans { get; set; }

        // ===== MAGANG =====
        public DbSet<Magang> PendaftaranMagang { get; set; }

        public DbSet<UserProfile> UserProfile { get; set; }

        // ===== NOTIFIKASI =====
        // Tambahkan ini agar Controller bisa mengakses tabel Notifications
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
        }
    }
}