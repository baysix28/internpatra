using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    // Ini nanti jadi TABEL di Database
    public class Pendaftaran
    {
        [Key] // Ini Primary Key (Wajib)
        public int Id { get; set; }
        
        // Memberikan '= string.Empty;' agar terhindar dari warning CS8618 (.NET 10 Nullable)
        public string NomorPendaftaran { get; set; } = string.Empty;

        public string Nama { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NoHp { get; set; } = string.Empty;
        public string TempatLahir { get; set; } = string.Empty;
        public DateTime? TglLahir { get; set; }
        
        // Kolom sosial media / file foto (bisa dikasih string.Empty atau dibuat nullable ? jika opsional)
        public string Instagram { get; set; } = string.Empty;
        public string PathFoto3x4 { get; set; } = string.Empty;

        public string Universitas { get; set; } = string.Empty;
        public string Fakultas { get; set; } = string.Empty;
        public string Jurusan { get; set; } = string.Empty;
        public string Nim { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string LokasiPenelitian { get; set; } = string.Empty;
        
        // Kolom spesifik penelitian (sudah benar dibuat nullable '?' agar fleksibel untuk anak magang)
        public string? JudulPenelitian { get; set; }
        public DateTime? TglMulai { get; set; }
        public DateTime? TglSelesai { get; set; }

        public string? TargetLokasi { get; set; }   // Tempat simpan lokasi Patra Niaga
        public string? TargetJurusan { get; set; }

        // --- BERKAS ADMISTRASI ---
        // Di Database, kita cuma simpan NAMA FILE-nya (String)
        public string? PathCV { get; set; }
        public string? PathProposal { get; set; }
        public string? PathSurat { get; set; }

        // Kolom tambahan otomatis (Bagus buat tracking admin)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Menunggu"; // Default: Menunggu Review
    }
}