using Microsoft.AspNetCore.Mvc;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Hosting; // Buat urus upload file
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace sinta_asp.Controllers
{
    // Class Dummy untuk Lowongan (Tetap dipakai buat Dashboard)
    public class LowonganKerja
    {
        public string? Title { get; set; }
        public string? Region { get; set; }
        public string? Company { get; set; } 
        public string? CompanyNameFull { get; set; }
        public string? Type { get; set; }
        public string? ImageUrl { get; set; }
        
        // --- INI YANG WAJIB DITAMBAHKAN BIAR GAK ERROR ---
        public string? Lokasi { get; set; }   
        public string? Jurusan { get; set; }
        public string? Description { get; set; } 
    }

    public class PenelitianController : Controller
    {
        private readonly AppDbContext _context; // Akses ke Database
        private readonly IWebHostEnvironment _webHostEnvironment; // Akses ke Folder wwwroot

        // Constructor: Kita minta "kunci" database & folder ke sistem
        public PenelitianController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 2. UPDATE DATA DUMMY (Isi Deskripsinya)
        private List<LowonganKerja> SemuaLowongan = new List<LowonganKerja>
        {
            // --- DATA KPI (KILANG) -> Fokus Jurusan & Proses ---
            new LowonganKerja { 
                Title = "Process Engineering", 
                Region = "Refinery Unit VI Balongan", 
                Company = "PT Kilang Pertamina Internasional", 
                Type = "Internship", 
                ImageUrl = "https://images.unsplash.com/photo-1581092921461-eab62e97a782?w=400",
                Jurusan = "Teknik Kimia / Fisika",
                Description = "Mempelajari proses distilasi minyak mentah dan monitoring unit operasi di kilang Balongan untuk menjaga kualitas produk BBM."
            },
            new LowonganKerja { 
                Title = "Mechanical Rotating", 
                Region = "Refinery Unit VI Balongan", 
                Company = "PT Kilang Pertamina Internasional", 
                Type = "Internship", 
                ImageUrl = "https://images.unsplash.com/photo-1504328345606-18bbc8c9d7d1?w=400",
                Jurusan = "Teknik Mesin",
                Description = "Fokus pada pemeliharaan dan analisis performa mesin rotasi seperti pompa, kompresor, dan turbin di area kilang."
            },

            // --- DATA PATRA NIAGA -> Fokus Lokasi & Distribusi ---
            new LowonganKerja { 
                Title = "Asset Operation MOR V", 
                Region = "Regional Jatimbalinus", 
                Company = "PT Pertamina Patra Niaga", 
                Type = "Penelitian", 
                ImageUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=400",
                Lokasi = "Integrated Terminal Jakarta (Plumpang)",
                Description = "Terminal BBM strategis dan terpenting di Indonesia yang menyuplai kebutuhan energi untuk wilayah Jabodetabek."
            },

            new LowonganKerja { 
                Title = "Supply Chain Management", 
                Region = "Regional Jawa Bagian Barat", 
                Company = "PT Pertamina Patra Niaga", 
                Type = "Internship", 
                ImageUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=400",
                Lokasi = "Integrated Terminal Jakarta (Plumpang)",
                Description = "Terminal BBM strategis dan terpenting di Indonesia yang menyuplai kebutuhan energi untuk wilayah Jabodetabek."
            },
            new LowonganKerja { 
                Title = "Aviation Sales", 
                Region = "Regional Jatimbalinus", 
                Company = "PT Pertamina Patra Niaga", 
                Type = "Full-Time", 
                ImageUrl = "https://images.unsplash.com/photo-1556761175-5973dc0f32e7?w=400",
                Lokasi = "DPPU Ngurah Rai Bali",
                Description = "Depot Pengisian Pesawat Udara (DPPU) tersibuk kedua di Indonesia, melayani avtur untuk penerbangan internasional."
            },
            new LowonganKerja { 
                Title = "IT Support", 
                Region = "Regional Jawa Bagian Tengah", 
                Company = "PT Pertamina Patra Niaga", 
                Type = "Internship", 
                ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400",
                Lokasi = "Kantor Cabang Semarang",
                Description = "Mendukung operasional IT dan infrastruktur jaringan untuk kelancaran distribusi energi di Jawa Tengah."
            },
             new LowonganKerja { 
                Title = "Legal & Relations", 
                Region = "Regional Kalimantan", 
                Company = "PT Pertamina Patra Niaga", 
                Type = "Internship", 
                ImageUrl = "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=400",
                Lokasi = "Fuel Terminal Balikpapan",
                Description = "Menangani aspek legalitas aset dan hubungan industrial di salah satu terminal BBM vital di Kalimantan."
            }
        };

        // 1. DASHBOARD UTAMA (Index)
        public IActionResult Index(string company, string search, string region)
        {
            var dataTampil = SemuaLowongan;

            if (!string.IsNullOrEmpty(company))
            {
                dataTampil = dataTampil.Where(x => x.Company != null && x.Company == company).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                dataTampil = dataTampil.Where(x => x.Title != null && x.Title.ToLower().Contains(search.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(region))
            {
                dataTampil = dataTampil.Where(x => x.Region != null && x.Region == region).ToList();
            }

            ViewData["SelectedCompany"] = company;
            ViewData["SelectedSearch"] = search;
            ViewData["SelectedRegion"] = region;

            return View(dataTampil);
        }

        // 2. TAMPILKAN FORM (GET)
        public IActionResult Daftar()
        {
            return View();
        }

        // 3. PROSES SUBMIT FORM (POST) -- INI LOGIC SIMPAN YANG BARU
        [HttpPost]
        public async Task<IActionResult> SubmitPendaftaran(PendaftaranPenelitianModel model)
        {
            if (ModelState.IsValid)
            {
                // A. Upload File Dulu
                string cvPath = await UploadFile(model.FileCV, "cv");
                string proposalPath = await UploadFile(model.FileProposal, "proposal");
                string suratPath = await UploadFile(model.FileSurat, "surat");

                // B. Pindahkan Data dari Model Form ke Model Database
                var pendaftaranBaru = new Pendaftaran
                {
                    Nama = model.Nama,
                    Email = model.Email,
                    NoHp = model.NoHp,
                    TempatLahir = model.TempatLahir,
                    TglLahir = model.TglLahir,
                    Instagram = model.Instagram,
                    
                    Universitas = model.Universitas,
                    Fakultas = model.Fakultas,
                    Jurusan = model.Jurusan,
                    Nim = model.Nim,

                    Company = model.Company,
                    Region = model.Region,
                    LokasiPenelitian = model.LokasiPenelitian,
                    JudulPenelitian = model.JudulPenelitian,
                    TglMulai = model.TglMulai,
                    TglSelesai = model.TglSelesai,

                    TargetLokasi = model.TargetLokasi,  
                    TargetJurusan = model.TargetJurusan,

                    // Simpan Lokasi Filenya saja
                    PathCV = cvPath,
                    PathProposal = proposalPath,
                    PathSurat = suratPath,
                    
                    CreatedAt = DateTime.Now,
                    Status = "Menunggu Review"
                };

                // C. Simpan ke Database
                _context.Pendaftarans.Add(pendaftaranBaru);
                await _context.SaveChangesAsync(); // <-- DETIK-DETIK MASUK DATABASE

                // D. Selesai, lempar ke halaman Sukses
                return RedirectToAction("Berhasil");
            }

            // Kalau error validasi, balikin ke form
            return View("Daftar", model);
        }

        // 4. HALAMAN SUKSES
        public IActionResult Berhasil()
        {
            return View();
        }

        // --- HELPER FUNCTION: CARA SIMPAN FILE BIAR RAPI ---
        private async Task<string?> UploadFile(Microsoft.AspNetCore.Http.IFormFile file, string jenis)
        {
            if (file == null || file.Length == 0) return null;

            // 1. Tentukan folder simpan: wwwroot/uploads/cv (misalnya)
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", jenis);
            
            // Buat folder kalau belum ada
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 2. Bikin nama file unik (biar gak bentrok kalau ada nama file sama)
            // Contoh: cv_jokowi_8374823.pdf
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 3. Salin file ke folder tujuan
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 4. Kembalikan path relatif untuk disimpan di database
            return "/uploads/" + jenis + "/" + uniqueFileName;
        }
    }
}