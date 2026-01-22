using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class PendaftaranPenelitianModel
    {
        // --- STEP 1: PERSONAL ---
        [Required(ErrorMessage = "Nama Lengkap wajib diisi.")]
        public string Nama { get; set; }
        
        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email salah.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "No HP wajib diisi.")]
        // Validasi Regex: Harus angka, diawali 08, panjang 10-13 digit
        [RegularExpression(@"^08[0-9]{8,11}$", ErrorMessage = "No HP harus diawali '08' (10-13 angka).")]
        public string NoHp { get; set; }
        
        [Required(ErrorMessage = "Tempat Lahir wajib diisi.")]
        public string TempatLahir { get; set; }
        
        [Required(ErrorMessage = "Tanggal Lahir wajib diisi.")]
        public DateTime? TglLahir { get; set; }
        
        [Required(ErrorMessage = "Username Instagram wajib diisi.")]
        public string Instagram { get; set; }

        [Required(ErrorMessage = "Upload foto 3x4")]
        public IFormFile Foto3x4 { get; set; }

        // --- STEP 2: EDUCATION ---
        [Required(ErrorMessage = "Universitas wajib diisi.")]
        public string Universitas { get; set; }
        
        [Required(ErrorMessage = "Fakultas wajib diisi.")]
        public string Fakultas { get; set; }
        
        [Required(ErrorMessage = "Jurusan wajib diisi.")]
        public string Jurusan { get; set; }
        
        [Required(ErrorMessage = "NIM wajib diisi.")]
        public string Nim { get; set; }

        // --- STEP 3: RESEARCH ---
        [Required(ErrorMessage = "Company Tujuan wajib diisi.")]
        public string Company { get; set; }
        
        [Required(ErrorMessage = "Region wajib dipilih.")]
        public string Region { get; set; }
        
        [Required(ErrorMessage = "Lokasi Penelitian wajib diisi.")]
        public string LokasiPenelitian { get; set; }
        
        [Required(ErrorMessage = "Judul Penelitian wajib diisi.")]
        public string JudulPenelitian { get; set; }
        
        [Required(ErrorMessage = "Tanggal Mulai wajib diisi.")]
        public DateTime? TglMulai { get; set; }
        
        [Required(ErrorMessage = "Tanggal Selesai wajib diisi.")]
        public DateTime? TglSelesai { get; set; }

        public string? TargetLokasi { get; set; }   
        public string? TargetJurusan { get; set; }

        // --- STEP 4: FILES ---
        [Required(ErrorMessage = "File CV wajib diupload.")]
        public IFormFile FileCV { get; set; }
        
        [Required(ErrorMessage = "File Proposal wajib diupload.")]
        public IFormFile FileProposal { get; set; }
        
        [Required(ErrorMessage = "Surat Pengantar wajib diupload.")]
        public IFormFile FileSurat { get; set; }
    }
}