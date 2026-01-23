using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    [Table("Mahasiswa")]
    public class Mahasiswa
    {
        [Key]
        public int Id { get; set; }
        public string? TipePendaftaran { get; set; }
        public string? FotoPath { get; set; }
        public string NamaLengkap { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TempatLahir { get; set; } = string.Empty;
        public DateTime? TanggalLahir { get; set; }
        public string NoHP { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;

        // PERBAIKAN DI SINI: Sesuaikan dengan kolom SSMS
        public string NamaKampus { get; set; } = string.Empty;

        public string Fakultas { get; set; } = string.Empty;
        public string Jurusan { get; set; } = string.Empty;
        public string NIM { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Lokasi { get; set; } = string.Empty;
        public string DetailTambahan { get; set; } = string.Empty;
        public DateTime? TanggalMulai { get; set; }
        public DateTime? TanggalSelesai { get; set; }
        public string? CVPath { get; set; }
        public string? ProposalPath { get; set; }
        public string? SuratKampusPath { get; set; }
        public string Status { get; set; } = "Pending";
    }
}