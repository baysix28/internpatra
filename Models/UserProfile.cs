using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        
        // Hubungkan ke tabel Users (Foreign Key)
        public int UserId { get; set; }

        public string NamaLengkap { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? NoHP { get; set; }
        
        public string? NamaPerguruanTinggi { get; set; }
        
        public string? FotoProfil { get; set; } 
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}