using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sinta_asp.Models
{
    // Ini nanti jadi TABEL di MySQL
    public class Pendaftaran
    {
        [Key] // Ini Primary Key (Wajib)
        public int Id { get; set; }

        public string Nama { get; set; }
        public string Email { get; set; }
        public string NoHp { get; set; }
        public string TempatLahir { get; set; }
        public DateTime? TglLahir { get; set; }
        public string Instagram { get; set; }

        public string Universitas { get; set; }
        public string Fakultas { get; set; }
        public string Jurusan { get; set; }
        public string Nim { get; set; }

        public string Company { get; set; }
        public string Region { get; set; }
        public string LokasiPenelitian { get; set; }
        public string? JudulPenelitian { get; set; }
        public DateTime? TglMulai { get; set; }
        public DateTime? TglSelesai { get; set; }

        public string? TargetLokasi { get; set; }   // Buat nyimpen lokasi Patra Niaga
        public string? TargetJurusan { get; set; }

        // --- BEDANYA DISINI ---
        // Di Database, kita cuma simpan NAMA FILE-nya (String), bukan File-nya.
        public string? PathCV { get; set; }
        public string? PathProposal { get; set; }
        public string? PathSurat { get; set; }

        // Kolom tambahan otomatis (Optional, bagus buat tracking)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Menunggu"; // Default: Menunggu Review
    }
}