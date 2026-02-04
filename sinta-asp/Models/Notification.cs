using System;
using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string? Nama { get; set; }
        public string? Lokasi { get; set; }
        public string? Type { get; set; } // 'new', 'expired', 'status_update'
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? ExternalId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}