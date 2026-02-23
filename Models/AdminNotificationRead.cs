using System;
using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class AdminNotificationRead
    {
        [Key]
        public int Id { get; set; }
        public int NotificationId { get; set; } // ID Notifikasi yang dibaca
        public int AdminId { get; set; }        // ID Admin yang membaca
        public DateTime ReadAt { get; set; } = DateTime.Now;
    }
}