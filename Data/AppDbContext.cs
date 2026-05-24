using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;
using sinta_asp.Areas.Admin.Models; // Namespace baru dari fix-web

namespace sinta_asp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ===== USER / ADMIN (Penting untuk Login) =====
        public DbSet<Admin> Admins { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Mahasiswa> Mahasiswa { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }

        // ===== PENDAFTARAN PENELITIAN & LOWONGAN (Prioritas vava4) =====
        public DbSet<Pendaftaran> Pendaftarans { get; set; }
        public DbSet<Lowongan> Lowongan { get; set; } // Milik Vava yang harus ada

        // ===== MAGANG =====
        public DbSet<Magang> PendaftaranMagang { get; set; }

        // ===== NOTIFIKASI (Fitur baru dari fix-web) =====
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }
        public DbSet<AdminNotificationRead> AdminNotificationReads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Konfigurasi khusus untuk tabel Notifikasi dari fix-web
            modelBuilder.Entity<AdminNotification>().ToTable("AdminNotifications");
        }
    }
}