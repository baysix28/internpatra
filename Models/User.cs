using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nama { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Peserta"; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- TAMBAHKAN DUA BARIS INI ---
        public bool IsEmailConfirmed { get; set; } = false; // Default: Belum diverifikasi
        public string? VerificationToken { get; set; }     // Kode unik untuk link email
    }
}