using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class Admin
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nama { get; set; } 
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(100)]
        public string RegionManaged { get; set; } 

        public string? SmtpPassword { get; set; }
    }
}