using Microsoft.AspNetCore.Mvc;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Hosting; // Buat urus upload file
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net;     
using System.Net.Mail; 


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

        // 3. PROSES SUBMIT FORM (POST) -- UPDATE TERBARU
        [HttpPost]
        public async Task<IActionResult> SubmitPendaftaran(PendaftaranPenelitianModel model)
        {
            // Hapus validasi untuk kolom yang diisi otomatis agar ModelState jadi "True"
            ModelState.Remove("NomorPendaftaran");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                // A. Upload File
                string fotoPath = await UploadFile(model.Foto3x4, "foto");
                string cvPath = await UploadFile(model.FileCV, "cv");
                string proposalPath = await UploadFile(model.FileProposal, "proposal");
                string suratPath = await UploadFile(model.FileSurat, "surat");

                // --- BARU: Generate Nomor Otomatis ---
                string nomorGenerated = GenerateNomorPendaftaran();

                // B. Pindahkan Data
                var pendaftaranBaru = new Pendaftaran
                {
                    // --- BARU: Masukkan Nomor ke Database ---
                    NomorPendaftaran = nomorGenerated, 

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

                    PathCV = cvPath,
                    PathProposal = proposalPath,
                    PathSurat = suratPath,
                    
                    CreatedAt = DateTime.Now,
                    Status = "Dalam Proses"
                };

                // C. Simpan ke Database
                _context.Pendaftarans.Add(pendaftaranBaru);
                await _context.SaveChangesAsync();

                // --- BARU: Kirim Email Notifikasi ---
                // (Pastikan method KirimEmailNotifikasi sudah di-copy di bawah)
                KirimEmailNotifikasi(model.Email, nomorGenerated, model.Nama);

                // D. Lempar ke halaman Berhasil sambil bawa datanya (biar bisa munculin nomor)
                return View("Berhasil", pendaftaranBaru);
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors) 
            {
                Console.WriteLine(error.ErrorMessage); // Error akan muncul di terminal dotnet watch
            }

            return View("Daftar", model);
        }

        // 4. HALAMAN SUKSES
        public IActionResult Berhasil()
        {
            return View();
        }

        // 5. CEK STATUS 
        // 1. Ini untuk menampilkan halaman form saat menu diklik
        [HttpGet]
        public IActionResult CekStatus()
        {
            return View(); // Mengembalikan halaman kosong tanpa data
        }

        // 2. Ini untuk memproses saat tombol "PERIKSA" diklik
        [HttpPost]
        public IActionResult CekStatus(string noPendaftaran)
        {
            // Cari data di database SINTA
            var data = _context.Pendaftarans.FirstOrDefault(x => x.NomorPendaftaran == noPendaftaran);
            
            if (data == null)
            {
                // Kirim pesan error kalau nomor tidak ada
                ViewBag.PesanError = "Nomor Pendaftaran tidak ditemukan. Silakan periksa kembali.";
            }

            // Kembalikan ke halaman yang SAMA, tapi kali ini bawa 'data' hasil pencarian
            return View(data); 
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

        // --- HELPER 1: GENERATE NOMOR PENDAFTARAN (PEN/2026/I/0001) ---
        private string GenerateNomorPendaftaran()
        {
            DateTime now = DateTime.Now;
            string tahun = now.Year.ToString();
            string bulanRomawi = GetBulanRomawi(now.Month);
            
            // Prefix untuk pencarian: PEN/2026/I/
            string prefix = $"PEN/{tahun}/{bulanRomawi}/";

            // Cek database untuk nomor terakhir dengan prefix yg sama
            var dataTerakhir = _context.Pendaftarans
                                .Where(x => x.NomorPendaftaran.StartsWith(prefix))
                                .OrderByDescending(x => x.NomorPendaftaran)
                                .FirstOrDefault();

            int urutan = 1;
            if (dataTerakhir != null)
            {
                // Jika ada (misal .../0015), ambil angka terakhir
                string[] parts = dataTerakhir.NomorPendaftaran.Split('/');
                string angkaTerakhir = parts[parts.Length - 1]; // ambil "0015"
                
                if (int.TryParse(angkaTerakhir, out int lastNumber))
                {
                    urutan = lastNumber + 1;
                }
            }

            // Format jadi 4 digit: 0001
            return $"{prefix}{urutan.ToString("D4")}";
        }

        // --- HELPER 2: UBAH ANGKA BULAN JADI ROMAWI ---
        private string GetBulanRomawi(int bulan)
        {
            string[] romawi = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII" };
            return (bulan >= 1 && bulan <= 12) ? romawi[bulan] : "";
        }

        // --- HELPER 3: KIRIM EMAIL (SMTP GMAIL) ---
        private void KirimEmailNotifikasi(string emailTujuan, string noPendaftaran, string nama)
        {
            try 
            {
                // GANTI DENGAN EMAIL ASLIMU NANTI
                string emailPengirim = "sintapertamina@gmail.com"; 
                string passwordApp = "cipjzsmrwrwhvtnv"; // Bukan password login biasa!

                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(emailPengirim, passwordApp);

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(emailPengirim, "Sistem Internship Pertamina");
                mail.To.Add(emailTujuan);
                mail.Subject = "Pendaftaran Magang Berhasil - ";
                
                string bodyEmail = $@"
                    <p>Yth. Sdr/i <b>{nama}</b>,</p>
                    <p>Pendaftaran penelitian Anda telah masuk dalam sistem dengan nomor pendaftaran:</p>
                    <p><b>{noPendaftaran}</b></p>
                    <p>Silakan tunggu email tanggapan dari kami atau periksa status penerimaan penelitian Anda melalui Web Sinta dengan memasukkan nomor pendaftaran tersebut.</p>
                    <p>
                        Salam hormat,<br/>
                        Human Capital<br/>
                        PT Pertamina Patra Niaga Regional Jawa Bagian Tengah
                    </p>
                    <hr/>
                    <p style='font-size: 11px; color: gray;'>*Email ini dikirimkan secara otomatis, mohon untuk <b>tidak membalas (do not reply)</b> email ini.</p>
                ";
                
                mail.Body = bodyEmail;
                mail.IsBodyHtml = true;

                client.Send(mail);
            }
            catch (Exception ex)
            {
                // Kalau email gagal, jangan bikin error aplikasi, cukup catat di console
                Console.WriteLine("Gagal kirim email: " + ex.Message);
            }
        }
    }
}