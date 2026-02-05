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
        public IActionResult Index(string search, string company, string region, int page = 1)
        {
            // 1. AUTO-SEED: Cek apakah Database kosong?
            if (!_context.Lowongan.Any()) 
            {
                var dataDummy = GetDummyData();
                _context.Lowongan.AddRange(dataDummy);
                _context.SaveChanges(); 
            }

            // 2. SUMBER DATA
            var allData = _context.Lowongan.ToList();
            
            // 2. LOGIKA FILTER
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
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

            // 3. LOGIKA PAGINATION
            int pageSize = 8; 
            int totalItems = allData.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var dataHalamanIni = allData
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // 4. MASUKKAN KE VIEWMODEL
            var model = new sinta_asp.Models.LowonganViewModel
            {
                Lowongan = dataHalamanIni,
                CurrentPage = page,
                TotalPages = totalPages
            };

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

        // 3. PROSES SUBMIT FORM (POST) -- SUDAH DIGABUNGKAN
        [HttpPost]
        public async Task<IActionResult> SubmitPendaftaran(PendaftaranPenelitianModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Ambil User Login (PENTING: Biar emailnya sesuai akun)
                var currentUserEmail = User.Identity?.Name;

                // 2. Upload File
                string fotoPath = await UploadFile(model.Foto3x4, "foto");
                string cvPath = await UploadFile(model.FileCV, "cv");
                string proposalPath = await UploadFile(model.FileProposal, "proposal");
                string suratPath = await UploadFile(model.FileSurat, "surat");

                // 3. Generate Nomor Otomatis
                string nomorGenerated = GenerateNomorPendaftaran();

                // 4. Pindahkan Data
                var pendaftaranBaru = new Pendaftaran
                {
                    NomorPendaftaran = nomorGenerated, 
                    Nama = model.Nama,
                    
                    // Gunakan email user yang login, kalau null pakai dari form (fallback)
                    Email = currentUserEmail ?? model.Email, 
                    
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
                    Status = "Menunggu Review"
                };

                // 5. Simpan Data Pendaftaran ke Database
                _context.Pendaftarans.Add(pendaftaranBaru);

                // 6. SIMPAN NOTIFIKASI (Fitur dari vava)
                // Kita bungkus try-catch biar kalau tabel Notification belum ada, pendaftaran tetap jalan
                try {
                    _context.Set<Notification>().Add(new Notification {
                        UserEmail = currentUserEmail ?? model.Email,
                        Title = "Pendaftaran Riset",
                        Message = $"Riset '{model.JudulPenelitian}' berhasil didaftarkan.",
                        Type = "Penelitian",
                        CreatedAt = DateTime.Now
                    });
                } catch {
                    // Abaikan error notifikasi jika tabel belum siap
                }

                // 7. Save Changes (Simpan ke SQL)
                await _context.SaveChangesAsync();

                // 8. Kirim Email Notifikasi (Fitur dari vava3/SEMUA)
                KirimEmailNotifikasi(model.Email, nomorGenerated, model.Nama);

                // 9. Lempar ke halaman Berhasil
                return View("Berhasil", pendaftaranBaru);
            }

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

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", jenis);
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + jenis + "/" + uniqueFileName;
        }

        // --- HELPER 1: GENERATE NOMOR PENDAFTARAN (PEN/2026/I/0001) ---
        private string GenerateNomorPendaftaran()
        {
            DateTime now = DateTime.Now;
            string tahun = now.Year.ToString();
            string bulanRomawi = GetBulanRomawi(now.Month);
            
            string prefix = $"PEN/{tahun}/{bulanRomawi}/";

            var dataTerakhir = _context.Pendaftarans
                                .Where(x => x.NomorPendaftaran.StartsWith(prefix))
                                .OrderByDescending(x => x.NomorPendaftaran)
                                .FirstOrDefault();

            int urutan = 1;
            if (dataTerakhir != null)
            {
                string[] parts = dataTerakhir.NomorPendaftaran.Split('/');
                string angkaTerakhir = parts[parts.Length - 1]; 
                
                if (int.TryParse(angkaTerakhir, out int lastNumber))
                {
                    urutan = lastNumber + 1;
                }
            }

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
                string emailPengirim = "sintapertamina@gmail.com"; 
                string passwordApp = "cipjzsmrwrwhvtnv"; 

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
                Console.WriteLine("Gagal kirim email: " + ex.Message);
            }
        }
    }
}