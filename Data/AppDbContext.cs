using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;
using sinta_asp.Areas.Admin.Models; // Namespace untuk AdminNotification

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

        public DbSet<Mahasiswa> Mahasiswa { get; set; }

        // ===== PENDAFTARAN PENELITIAN =====
        public DbSet<Pendaftaran> Pendaftarans { get; set; }

        // ===== MAGANG =====
        public DbSet<Magang> PendaftaranMagang { get; set; }

        public DbSet<UserProfile> UserProfile { get; set; }

        // ===== NOTIFIKASI =====
        // Notifikasi untuk sisi Mahasiswa/User
        public DbSet<Notification> Notifications { get; set; }

        // Notifikasi untuk sisi Admin (Dashboard & Sidebar)
        public DbSet<AdminNotification> AdminNotifications { get; set; }
        public DbSet<AdminNotificationRead> AdminNotificationReads { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Opsional: Memastikan nama tabel di database sesuai
            modelBuilder.Entity<AdminNotification>().ToTable("AdminNotifications");
        }
    }
}