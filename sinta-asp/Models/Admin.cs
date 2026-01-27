using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class Admin
    {
        [Key] 
        public int Id { get; set; }

        [Required] // Tidak boleh kosong
        [StringLength(100)]
        public string Nama { get; set; }

        [Required]
        [EmailAddress] // Validasi format email
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}