using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    [Table("pendaftaran_magang")]
    public class Magang
    {
        [Key]
        public int Id { get; set; }

        public string? FotoProfil { get; set; }
        [Required] public string NamaLengkap { get; set; } = "";
        [Required, EmailAddress] public string EmailPribadi { get; set; } = "";
        public string? TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }
        public string? NoHp { get; set; }
        public string? Instagram { get; set; }

        public string? NamaPerguruanTinggi { get; set; }
        public string? Fakultas { get; set; }
        public string? Jurusan { get; set; }
        public string? NIM { get; set; }

        public string? Company { get; set; }
        public string? Region { get; set; }
        public string? Lokasi { get; set; }
        public string? RekomendasiPegawai { get; set; }
        public DateTime MulaiMagang { get; set; }
        public DateTime SelesaiMagang { get; set; }

        public string? FileCv { get; set; }
        public string? FileSuratPengantar { get; set; }
        public string? FileProposal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
