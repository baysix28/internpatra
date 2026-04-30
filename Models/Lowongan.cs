using System;
using System.ComponentModel.DataAnnotations; // Untuk validasi (Key, Required)

namespace sinta_asp.Models
{
    public class Lowongan
    {
        [Key] // Menandakan ini Primary Key
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        
        public string ImageUrl { get; set; }

        // INI PENTING UNTUK FILTER
        public string Company { get; set; } // Contoh: "PT Pertamina Patra Niaga"
        public string Region { get; set; }  // Contoh: "Regional Jawa Bagian Tengah"

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Otomatis isi tanggal sekarang
    }
}