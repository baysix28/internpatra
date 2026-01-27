using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    [Table("Admins")]
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required] 
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Nama { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}