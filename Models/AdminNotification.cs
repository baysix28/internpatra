using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    [Table("admin_notifications")]
    public class AdminNotification
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; } // Contoh: "Pendaftaran Baru" atau "Masa Magang Berakhir"
        public string? Message { get; set; } // Contoh: "Budi mendaftar di Unit VI Balongan"
        
        // Type untuk membedakan icon di layout (misal: "Baru" atau "Selesai")
        public string? Type { get; set; } 

        // TargetRegion digunakan agar Admin Region hanya melihat notif regionnya sendiri
        public string? TargetRegion { get; set; } 

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relasi opsional ke pendaftaran_magang (agar bisa klik langsung ke detail)
        public int? MagangId { get; set; }
    }
}