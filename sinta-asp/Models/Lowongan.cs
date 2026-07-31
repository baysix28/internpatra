using System;
using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class Lowongan
    {
        [Key]
        public int Id { get; set; }
        
        public string? Judul { get; set; }
        public string? Deskripsi { get; set; }
        public DateTime? TanggalPosting { get; set; } = DateTime.Now;

        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Company { get; set; }
        public string? Region { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}