using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    [Table("Users")] // Nama tabel di SQL Server nanti adalah 'Users'
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama wajib diisi")]
        public string Nama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}