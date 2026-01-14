using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class PendaftaranRequest
    {
        // Data Diri
        public IFormFile? FotoProfil { get; set; }
        public string? NamaLengkap { get; set; }
        public string? EmailPribadi { get; set; }
        public string? TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }
        public string? NoHp { get; set; }
        public string? Instagram { get; set; }
        // public string? AlamatLengkap { get; set; }

        // Data Kampus
        public string? NamaPerguruanTinggi { get; set; }
        public string? Fakultas { get; set; }
        public string? Jurusan { get; set; }
        public int Semester { get; set; }
        public decimal Ipk { get; set; }
        public string? Nim { get; set; }

        // Data Magang
        public string? Company { get; set; }
        public string? Region { get; set; }
        public string? Lokasi { get; set; }
        public string? RekomendasiPegawai { get; set; }
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalSelesai { get; set; }

        // File Pendukung (Uploads)
        public IFormFile? FileCv { get; set; }
        public IFormFile? FilePengantar { get; set; }
        public IFormFile? FileProposal { get; set; }
    }
}