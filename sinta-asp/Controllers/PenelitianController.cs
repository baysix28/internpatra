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
        // Ubah tipe return-nya jadi List<Lowongan>
        // GANTI FUNGSI GETDUMMYDATA DI BAWAH DENGAN INI
        private List<Lowongan> GetDummyData() 
        {
            return new List<Lowongan>
            {
                // --- DATA KPI (KILANG) ---
                new Lowongan { 
                    Title = "Akuntansi/ Ekonomi & Bisnis", 
                    Region = "Refinery Unit VI Balongan", 
                    Company = "PT Kilang Pertamina Internasional (KPI)", 
                    ImageUrl = "https://images.unsplash.com/photo-1581092921461-eab62e97a782?w=400",
                    Description = "Mempelajari proses distilasi minyak mentah dan monitoring unit operasi di kilang Balongan untuk menjaga kualitas produk BBM.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan { 
                    Title = "Elektro (Arus Kuat)", 
                    Region = "Refinery Unit VI Balongan", 
                    Company = "PT Kilang Pertamina Internasional (KPI)", 
                    ImageUrl = "https://images.unsplash.com/photo-1504328345606-18bbc8c9d7d1?w=400",
                    Description = "Fokus pada pemeliharaan dan analisis performa mesin rotasi seperti pompa, kompresor, dan turbin di area kilang.",
                    CreatedAt = DateTime.Now
                },

                // --- DATA PATRA NIAGA ---
                new Lowongan { 
                    Title = "Asset Operation MOR V", 
                    Region = "Regional Jatimbalinus", 
                    Company = "PT Pertamina Patra Niaga (C&T)", 
                    ImageUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=400",
                    Description = "Terminal BBM strategis dan terpenting di Indonesia yang menyuplai kebutuhan energi untuk wilayah Jabodetabek.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan { 
                    Title = "Asset Operation JBB", 
                    Region = "Regional Jawa Bagian Barat", 
                    Company = "PT Pertamina Patra Niaga (C&T)", 
                    ImageUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=400",
                    Description = "Terminal BBM strategis dan terpenting di Indonesia yang menyuplai kebutuhan energi untuk wilayah Jabodetabek.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan { 
                    Title = "Aviation FT Babullah", 
                    Region = "Regional Maluku Papua", 
                    Company = "PT Pertamina Patra Niaga (C&T)", 
                    ImageUrl = "https://images.unsplash.com/photo-1556761175-5973dc0f32e7?w=400",
                    Description = "Depot Pengisian Pesawat Udara (DPPU) tersibuk kedua di Indonesia, melayani avtur untuk penerbangan internasional.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan { 
                    Title = "Kantor Unit - SSC ICT V JBT", 
                    Region = "Regional Jawa Bagian Tengah", 
                    Company = "PT Pertamina Patra Niaga (C&T)", 
                    ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400",
                    Description = "Mendukung operasional IT dan infrastruktur jaringan untuk kelancaran distribusi energi di Jawa Tengah.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan { 
                    Title = "Asset Operation Region Sumbagut - Kantor Unit", 
                    Region = "Regional Sumbagut", 
                    Company = "PT Pertamina Patra Niaga (C&T)", 
                    ImageUrl = "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=400",
                    Description = "Menangani aspek legalitas aset dan hubungan industrial di salah satu terminal BBM vital di Kalimantan.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan { 
                    Title = "DPPU APT Pranoto", 
                    Region = "Regional Kalimantan", 
                    Company = "PT Pertamina Patra Niaga (C&T)", 
                    ImageUrl = "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=400",
                    Description = "Menangani aspek legalitas aset dan hubungan industrial di salah satu terminal BBM vital di Kalimantan.",
                    CreatedAt = DateTime.Now
                }
            };
        }

        // 1. DASHBOARD UTAMA (Index)
        // --- GANTI METHOD INDEX YANG LAMA DENGAN INI ---
        // Tambahkan parameter 'page' dengan default 1
        // Pastikan HANYA ADA SATU method Index seperti ini:
        public IActionResult Index(string search, string company, string region, int page = 1)
        {
            // 1. AUTO-SEED: Cek apakah Database kosong?
            if (!_context.Lowongan.Any()) 
            {
                // Kalau kosong, ambil data dummy yang di bawah
                var dataDummy = GetDummyData();
                
                // Masukkan ke Database SQL Server
                _context.Lowongan.AddRange(dataDummy);
                _context.SaveChanges(); // Simpan permanen
            }

            // 2. SUMBER DATA: Sekarang ambil dari Database (Bukan Dummy lagi)
            // Kita pakai .ToList() biar kode filter di bawahnya tidak perlu diubah sama sekali
            var allData = _context.Lowongan.ToList();
            
            // 2. LOGIKA FILTER (Search & Filter)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                // Mencari di Judul atau Nama Perusahaan
                allData = allData.Where(x => (x.Title != null && x.Title.ToLower().Contains(search)) || 
                                            (x.Company != null && x.Company.ToLower().Contains(search))).ToList();
            }

            // Filter Company
            if (!string.IsNullOrEmpty(company) && company != "All")
            {
                allData = allData.Where(x => x.Company != null && x.Company.Contains(company)).ToList();
            }

            // Filter Region
            if (!string.IsNullOrEmpty(region) && region != "All")
            {
                allData = allData.Where(x => x.Region == region).ToList();
            }

            // 3. LOGIKA PAGINATION (Matematika Halaman)
            int pageSize = 8; // Menampilkan 9 kartu per halaman
            int totalItems = allData.Count;
            
            // Hitung total halaman (dibulatkan ke atas)
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Mencegah error jika user mengetik halaman minus atau berlebih
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // POTONG DATA: Lewati halaman sebelumnya, Ambil 9 data
            var dataHalamanIni = allData
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // 4. MASUKKAN KE VIEWMODEL (Wadah Baru)
            var model = new sinta_asp.Models.LowonganViewModel
            {
                Lowongan = dataHalamanIni,
                CurrentPage = page,
                TotalPages = totalPages
            };

            // Simpan filter agar tidak hilang saat klik halaman berikutnya
            ViewData["SelectedSearch"] = search;
            ViewData["SelectedCompany"] = company;
            ViewData["SelectedRegion"] = region;

            return View(model);
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
                string fotoPath = await UploadFile(model.Foto3x4, "foto");
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
                    PathFoto3x4 = fotoPath,
                    
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