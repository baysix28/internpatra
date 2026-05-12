using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;
using System.Linq;
using System.Security.Claims;
using System.IO;

namespace sinta_asp.Controllers
{
    [Authorize(Policy = "PesertaOnly")]
    public class DashboardPesertaController : Controller
    {
        private readonly AppDbContext _context;
        // private readonly IWebHostEnvironment _environment;

        public DashboardPesertaController(AppDbContext context)
        {
            _context = context;
            // _environment = environment; // Tambahkan ini
        }

    public async Task<IActionResult> Index()
    {
        var userEmail = User.Identity?.Name;

        var profil = await _context.UserProfile
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        var riwayatMagang = await _context.PendaftaranMagang
            .Where(m => m.EmailPribadi == userEmail)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        // --- INOVASI: AMBIL DATA MAGANG AKTIF (TERBARU & DITERIMA) ---
        var today = DateTime.Today;

        var magangDiterima = riwayatMagang
            .Where(m => m.Status == "Diterima")
            .OrderByDescending(m => m.SelesaiMagang)
            .FirstOrDefault();

        ViewBag.MagangAktif = null;
        ViewBag.MagangAkanDatang = null;
        ViewBag.MagangSelesai = null;

        if (magangDiterima != null)
        {
            if (magangDiterima.MulaiMagang.Date > today)
                ViewBag.MagangAkanDatang = magangDiterima;
            else if (magangDiterima.SelesaiMagang.Date < today)
                ViewBag.MagangSelesai = magangDiterima;
            else
                ViewBag.MagangAktif = magangDiterima;
        }

        // --- LOGIKA PERHITUNGAN UNTUK DASHBOARD ---
        ViewBag.TotalMagang = riwayatMagang.Count;
        ViewBag.Menunggu = riwayatMagang.Count(m => m.Status == "Menunggu" || m.Status == "Review Berkas");
        ViewBag.Diterima = riwayatMagang.Count(m => m.Status == "Diterima");
        ViewBag.Ditolak = riwayatMagang.Count(m => m.Status == "Ditolak");
        
        // Menghitung jumlah status revisi
        ViewBag.Revisi = riwayatMagang.Count(m => m.Status == "Revisi");

        ViewBag.RiwayatMagang = riwayatMagang;
        return View(profil);
    }
        [HttpGet]
        public IActionResult GetInformasiTersedia()
        {
            var data = new List<object>();

            // =========================
            // MASTER DATA MAGANG - KPI
            // =========================
            var dataKPI = new Dictionary<string, List<string>>
            {
                {
                    "Refinery Unit VI Balongan", new List<string>
                    {
                        "Akuntansi / Ekonomi & Bisnis",
                        "Elektro (Arus Kuat)",
                        "Elektro (Arus Lemah)",
                        "Emergency & Insurance",
                        "Health",
                        "Hukum",
                        "Ilmu Komunikasi / FISIP / Administrasi Publik",
                        "Internal Audit",
                        "Kelautan / Perkapalan",
                        "Kimia Murni / MIPA",
                        "Konversi Energi / Migas / Kimia Air Bersih / Blanding / Loading",
                        "Logistik / Pergudangan / Procurement",
                        "Manajemen / SDM / Psikologi",
                        "Metalurgi / Material / Dirgantara",
                        "Safety (K3) / SMK3",
                        "Teknik Fisika",
                        "Teknik Industri",
                        "Teknik Informatika",
                        "Teknik Kimia",
                        "Teknik Lingkungan",
                        "Teknik Mesin",
                        "Teknik Mesin (Rotating)",
                        "Teknik Sipil"
                    }
                }
            };

            // =========================
            // MASTER DATA MAGANG - PPN
            // =========================
            var dataPPN = new Dictionary<string, List<string>>
            {
                {
                    "Regional Jatimbalinus", new List<string>
                    {
                        "Asset Operation MOR V","Bitumen Plant Gresik","C&T IA Jatimbalinus","Comm, Rel, & CSR MOR V",
                        "Corporate Operation & Service Region V","Corporate Sales Region V","DPPU BIL","DPPU Eltari Group",
                        "DPPU Iswahyudi","DPPU Juanda","DPPU Ngurah Rai","Finance MOR V","Fuel Terminal Atapupu",
                        "Fuel Terminal Badas","Fuel Terminal Bima","Fuel Terminal Camplong","Fuel Terminal Ende",
                        "Fuel Terminal Kalabahi","Fuel Terminal Madiun","Fuel Terminal Malang","Fuel Terminal Maumere",
                        "Fuel Terminal Reo","Fuel Terminal Sanggaran","Fuel Terminal Tenau","Fuel Terminal Tuban",
                        "Fuel Terminal Waingapu","HC Jatimbalinus","HSSE Region V","Integrated Terminal Ampenan",
                        "Integrated Terminal Manggis","Integrated Terminal Surabaya","Integrated Terminal T. Wangi",
                        "Legal Counsel Regional Jatimbalinus","Marine Region V","Medical Jatimbalinus",
                        "Procurement MOR V","Rel & Project Dev Region V","Retail Bali","Retail Kediri",
                        "Retail Malang","Retail NTB","Retail NTT","Retail Sales Region V","Retail Surabaya",
                        "S&D Region V","SSC ICT VI Jatimbalinus","XXX"
                    }
                },
                {
                    "Regional Jawa Bagian Barat", new List<string>
                    {
                        "Asset Operation JBB","Corp. Opt & Serv JBB","Corporate Sales JBB","DPPU Halim PK Group",
                        "DPPU Husein Sastranegara","DPPU Kertajati","Finance JBB","Fuel Terminal Bandung Group",
                        "Fuel Terminal Cikampek","Fuel Terminal Tasikmalaya","Fuel Terminal Tg Gerem","HSSE JBB",
                        "Human Capital","Integrated Terminal Balongan","Integrated Terminal Jakarta","Legal Counsel JBB",
                        "Medical JBB","MWH & LPG Cylinder","Procurement JBB","Reliability & Project Dev JBB",
                        "SA Retail Bandung","SA Retail Cirebon","SA Retail Karawang","SA Retail Sukabumi",
                        "SAM Retail Banten","SAM Retail Jabode","SHAFTHI","SHIPS","SCC ICT JBB",
                        "Supply & Distribution JBB","Unit Comm, Rel & CSR JBB"
                    }
                },
                {
                    "Regional Jawa Bagian Tengah", new List<string>
                    {
                        "AFT Adi Sumarmo","AFT Adi Sucipto","AFT Ahmad Yani","AFT YIA",
                        "Fuel Terminal Boyolali","Fuel Terminal Lomanis","Fuel Terminal Maos",
                        "Fuel Terminal Rewulu","Fuel Terminal Tegal","Integrated Terminal Cilacap",
                        "Integrated Terminal Semarang","Kantor Branch Marketing DIY & Surakarta",
                        "Kantor Unit - Asset Operation JBT","Kantor Unit - Comm, Rel & CSR JBT",
                        "Kantor Unit - Corp Operation & Serv JBT","Kantor Unit - Corporate Sales JBT",
                        "Kantor Unit - Finance JBT","Kantor Unit - HC JBT","Kantor Unit - HSSE JBT",
                        "Kantor Unit - Internal Audit","Kantor Unit - Legal Counsel JBT",
                        "Kantor Unit - Medical JBT","Kantor Unit - Operational Risk JBT",
                        "Kantor Unit - Procurement JBT","Kantor Unit - Rel & Project Dev JBT",
                        "Kantor Unit - Retail Sales JBT","Kantor Unit - SSC ICT V JBT",
                        "Kantor Unit - Supply & Distribution JBT"
                    }
                },
                {
                    "Regional Kalimantan", new List<string>
                    {
                        "DPPU APT Pranoto","DPPU H. Asan","DPPU Iskandar","DPPU Juwata",
                        "DPPU Kalimaru","DPPU Sepinggan","DPPU Supadio","DPPU Syamsudin Noor",
                        "DPPU Tjilik Riwut","Fuel Terminal Pulang Pisau","Fuel Terminal Kotabaru",
                        "Fuel Terminal Pangkalan Bun","Fuel Terminal Samarinda","Fuel Terminal Sampit",
                        "Fuel Terminal Sintang","Fuel Terminal Tarakan","Integrated Terminal Balikpapan",
                        "Integrated Terminal Banjarmasin","Integrated Terminal Pontianak",
                        "Kantor Patra Niaga Region Kalimantan","SAM Retail Kalbar","SAM Retail Kalselteng",
                        "SAM Retail Kaltimut"
                    }
                },
                {
                    "Regional Maluku Papua", new List<string>
                    {
                        "Aviation FT Babullah","Aviation FT Deo","Aviation FT Depati Mopah",
                        "Aviation FT Depati Rendani","Aviation FT Dumatubun","Aviation FT Frans Kaisiepo",
                        "Aviation FT Mathilda","Aviation FT Mozes Kilangin","Aviation FT Paniai",
                        "Aviation FT Pattimura","Aviation FT Sentani","Aviation FT Utarom",
                        "FT Biak","FT Bula","FT Dobo","FT Fak-Fak","FT Kaimana","FT Labuha",
                        "FT Manokwari","FT Masohi","FT Merauke","FT Nabire","FT Namlea",
                        "FT Sanana","FT Saumlaki","FT Serui","FT Sorong","FT Ternate",
                        "FT Tobelo","FT Tual","IT Jayapura","IT Wayame",
                        "Kantor Region - Asset Operation Papua-Maluku",
                        "Kantor Region - Comm, Rel & CSR Papua-Maluku",
                        "Kantor Region - Corp Operation & Serv Papua-Maluku",
                        "Kantor Region - Corporate Sales Papua-Maluku",
                        "Kantor Region - Finance Papua-Maluku",
                        "Kantor Region - HC Papua-Maluku",
                        "Kantor Region - HSSE Papua-Maluku",
                        "Kantor Region - Legal Counsel Papua-Maluku",
                        "Kantor Region - Medical Papua-Maluku",
                        "Kantor Region - Procurement Papua-Maluku",
                        "Kantor Region - Rel & Project Dev Papua-Maluku",
                        "Kantor Region - Retail Sales Papua-Maluku",
                        "Kantor Region - Supply & Dist Papua-Maluku",
                        "Sales Area Ambon"
                    }
                },
                {
                    "Regional Sumbagut", new List<string>
                    {
                        "Asset Operation Region Sumbagut","Branch Marketing Aceh",
                        "Branch Marketing Kepulauan Riau","Branch Marketing Sibolga",
                        "Branch Marketing Sumbar","Communication & CSR Region Sumbagut",
                        "Corp Operation & Serv Region Sumbagut","Corporate Sales Region Sumbagut",
                        "DPPU Hang Nadim Group","DPPU Kualanamu Group","DPPU Minangkabau",
                        "DPPU SIM","DPPU SSK II","Finance Region Sumbagut",
                        "Fuel Terminal Batam","Fuel Terminal Gunung Sitoli",
                        "Fuel Terminal Kijang Group","Fuel Terminal Kisaran",
                        "Fuel Terminal Krueng Raya","Fuel Terminal Medan Group",
                        "Fuel Terminal Meulaboh","Fuel Terminal Natuna Group",
                        "Fuel Terminal Pematang Siantar","Fuel Terminal Sabang",
                        "Fuel Terminal Sei Siak","Fuel Terminal Sibolga",
                        "Fuel Terminal Simeulue","Fuel Terminal Tembilahan",
                        "HC Region Sumbagut","HSSE Region Sumbagut",
                        "IA Region I","Integrated Terminal Dumai",
                        "Integrated Terminal Lhokseumawe","Integrated Terminal Tanjung Uban",
                        "Integrated Terminal Teluk Kabung","Legal Counsel Region Sumbagut",
                        "Medical Region Sumbagut","Procurement Region Sumbagut",
                        "Rel & Project Dev Region Sumbagut",
                        "Retail Sales Region Sumbagut",
                        "SSC ICT I Region Sumbagut",
                        "Supply & Distribution Region Sumbagut"
                    }
                }
            };

            foreach (var region in dataKPI)
            {
                foreach (var posisi in region.Value)
                {
                    data.Add(new
                    {
                        companyCode = "KPI",
                        companyName = "PT Kilang Pertamina Internasional",
                        region = region.Key,
                        lokasi = posisi,
                        status = "Tersedia"
                    });
                }
            }

            foreach (var region in dataPPN)
            {
                foreach (var posisi in region.Value)
                {
                    data.Add(new
                    {
                        companyCode = "PPN",
                        companyName = "PT Pertamina Patra Niaga",
                        region = region.Key,
                        lokasi = posisi,
                        status = "Tersedia"
                    });
                }
            }

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> InformasiTersedia()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRevisi(
            int id,
            string? nim,
            string? noHp,
            IFormFile? fileCv,
            IFormFile? fileSurat,
            IFormFile? fileProposal,
            string? mulaiMagang,
            string? selesaiMagang)
        {
            try
            {
                var userEmail = User.Identity?.Name;

                // Ambil data magang milik user ini saja (keamanan)
                var data = await _context.PendaftaranMagang
                    .FirstOrDefaultAsync(m => m.Id == id && m.EmailPribadi == userEmail);

                if (data == null)
                    return Json(new { success = false, message = "Data tidak ditemukan." });

                if (data.Status != "Revisi")
                    return Json(new { success = false, message = "Status bukan Revisi, tidak bisa diubah." });

                // Parse field revisi yang diminta admin jadi list lowercase
                var revisiFields = (data.RevisiFields ?? "")
                    .Split(',')
                    .Select(x => x.Trim().ToLower())
                    .ToList();

                // --- Update field sesuai yang diminta admin saja ---

                if (revisiFields.Any(r => r.Contains("akademik")) && !string.IsNullOrEmpty(nim))
                    data.NIM = nim;

                if (revisiFields.Any(r => r.Contains("kontak")) && !string.IsNullOrEmpty(noHp))
                    data.NoHp = noHp;

                if (revisiFields.Any(r => r.Contains("durasi")))
                {
                    if (DateTime.TryParse(mulaiMagang, out var tglMulai))
                        data.MulaiMagang = tglMulai;
                    if (DateTime.TryParse(selesaiMagang, out var tglSelesai))
                        data.SelesaiMagang = tglSelesai;
                }

                // --- Upload file jika diminta ---
                string uploadBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (revisiFields.Any(r => r.Contains("cv")) && fileCv != null && fileCv.Length > 0)
                {
                    string folder = Path.Combine(uploadBase, "cv");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    // Hapus file lama
                    if (!string.IsNullOrEmpty(data.FileCv))
                    {
                        var oldPath = Path.Combine(folder, Path.GetFileName(data.FileCv));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string fileName = $"cv_{Guid.NewGuid()}.pdf";
                    using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                    await fileCv.CopyToAsync(stream);
                    data.FileCv = "uploads/cv/" + fileName;
                }

                if (revisiFields.Any(r => r.Contains("surat")) && fileSurat != null && fileSurat.Length > 0)
                {
                    string folder = Path.Combine(uploadBase, "surat");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    if (!string.IsNullOrEmpty(data.FileSuratPengantar))
                    {
                        var oldPath = Path.Combine(folder, Path.GetFileName(data.FileSuratPengantar));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string fileName = $"surat_{Guid.NewGuid()}.pdf";
                    using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                    await fileSurat.CopyToAsync(stream);
                    data.FileSuratPengantar = "uploads/surat/" + fileName;
                }

                if (revisiFields.Any(r => r.Contains("proposal")) && fileProposal != null && fileProposal.Length > 0)
                {
                    string folder = Path.Combine(uploadBase, "proposal");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    if (!string.IsNullOrEmpty(data.FileProposal))
                    {
                        var oldPath = Path.Combine(folder, Path.GetFileName(data.FileProposal));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string fileName = $"proposal_{Guid.NewGuid()}.pdf";
                    using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                    await fileProposal.CopyToAsync(stream);
                    data.FileProposal = "uploads/proposal/" + fileName;
                }

                // --- Selesai revisi: kembalikan ke Menunggu & bersihkan field revisi ---
                data.Status = "Menunggu";
                data.RevisiFields = null;
                data.CatatanRevisi = null;

                _context.PendaftaranMagang.Update(data);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfil(string nama, string noHp, string univ)
        {
            try 
            {
                var userEmail = User.Identity?.Name;
                var profile = await _context.UserProfile.FirstOrDefaultAsync(m => m.Email == userEmail);

                if (profile != null)
                {
                    profile.NamaLengkap = nama;
                    profile.NoHP = noHp;
                    profile.NamaPerguruanTinggi = univ;
                    profile.UpdatedAt = DateTime.Now;
                    _context.UserProfile.Update(profile);
                }
                else 
                {
                    var newProfile = new UserProfile {
                        Email = userEmail ?? "",
                        NamaLengkap = nama,
                        NoHP = noHp,
                        NamaPerguruanTinggi = univ,
                        UserId = 0 
                    };
                    _context.UserProfile.Add(newProfile);
                }

                await _context.SaveChangesAsync(); 
                return Json(new { success = true, message = "Profil berhasil disimpan!" });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdatePassword(string oldPass, string newPass) // Pastikan nama ini sama dengan di AJAX
        {
            try 
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Json(new { success = false, message = "Sesi habis, silakan login ulang." });
                }

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null) 
                {
                    return Json(new { success = false, message = "User tidak ditemukan." });
                }

                // Cek apakah data dari AJAX masuk atau null
                if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
                {
                    return Json(new { success = false, message = "Data tidak diterima oleh server." });
                }

                if (user.Password.Trim() != oldPass.Trim())
                {
                    return Json(new { success = false, message = "Password lama salah!" });
                }

                user.Password = newPass.Trim();
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Ini akan membantu kamu melihat error apa yang terjadi di server via Debugger
                return Json(new { success = false, message = "Error Server: " + ex.Message });
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> UpdateFoto(IFormFile fotoProfil)
        {
            if (fotoProfil == null || fotoProfil.Length == 0)
                return Json(new { success = false, message = "File tidak ditemukan" });

            try
            {
                var userEmail = User.Identity?.Name;
                
                // 1. Tentukan folder (Gunakan Path.Combine agar aman)
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // 2. Nama File Unik
                string extension = Path.GetExtension(fotoProfil.FileName);
                string fileName = $"profile_{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(folderPath, fileName);

                // 3. Simpan File Fisik
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await fotoProfil.CopyToAsync(stream);
                }

                // 4. Update Database
                var profile = await _context.UserProfile.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (profile != null) 
                {
                    // Hapus foto lama dari folder jika ada
                    if (!string.IsNullOrEmpty(profile.FotoProfil))
                    {
                        string oldPath = Path.Combine(folderPath, profile.FotoProfil);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    profile.FotoProfil = fileName;
                    _context.UserProfile.Update(profile);
                    await _context.SaveChangesAsync();
                }

                // Return path lengkap untuk preview di Frontend
                return Json(new { success = true, fileName = fileName, filePath = "/uploads/profile/" + fileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFoto()
        {
            var userEmail = User.Identity?.Name;
            var profile = await _context.UserProfile.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (profile != null && !string.IsNullOrEmpty(profile.FotoProfil))
            {
                // Hapus file fisik
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile", profile.FotoProfil);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                profile.FotoProfil = null;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        private string CalculateTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalMinutes < 1) return "Baru saja";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} menit lalu";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} jam lalu";
            return dt.ToString("dd MMM yyyy");
        }
    }
}