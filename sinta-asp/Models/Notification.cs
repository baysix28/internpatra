using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false; // Gunakan = bukan -
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Gunakan = bukan -
        public string UserEmail { get; set; } = string.Empty;
    }
}