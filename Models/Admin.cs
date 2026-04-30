using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    public class Admin
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nama { get; set; } = "";
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        // Properti Baru
        [Required]
        public string Password { get; set; } = "";

        // ALIAS UNTUK MEMPERBAIKI ERROR CONTROLLER LAMA
        [NotMapped]
        public string PasswordHash 
        { 
            get => Password; 
            set => Password = value; 
        }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "AdminRegion"; 

        // Properti Baru
        [StringLength(100)]
        public string? Region { get; set; } 

        // ALIAS UNTUK MEMPERBAIKI ERROR CONTROLLER LAMA
        [NotMapped]
        public string? RegionManaged 
        { 
            get => Region; 
            set => Region = value; 
        }

        public string? ActivationToken { get; set; }
        public bool IsActive { get; set; } = false;
        public string? SmtpPassword { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}