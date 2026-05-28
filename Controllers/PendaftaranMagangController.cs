using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authorization;
using sinta_asp.Services;

namespace sinta_asp.Controllers
{
    [Authorize(AuthenticationSchemes = "PesertaScheme")] 
    public class PendaftaranMagangController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;

        public PendaftaranMagangController(
            AppDbContext context, 
            IWebHostEnvironment environment,
            IEmailService emailService)
        {
            _context = context;
            _environment = environment;
            _emailService = emailService;
        }

        
        [AllowAnonymous] // ← tambahkan ini

        public IActionResult Index() => View();

        [Authorize(AuthenticationSchemes = "PesertaScheme")]
        public IActionResult DataMagang()
        {
            string email = User.Identity!.Name!;

            var existing = _context.PendaftaranMagang
                .FirstOrDefault(x => x.EmailPribadi == email && x.Status == "Draft");

            if (existing != null)
            {
                return View(existing); // draft milik akun ini
            }

            return View(new Magang
            {
                EmailPribadi = email
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailMagang(int id)
        {
            var m = await _context.PendaftaranMagang.FindAsync(id);

            if (m == null)
            {
                return NotFound();
            }

            return Json(new {
                nama = m.NamaLengkap,
                email = m.EmailPribadi,
                wa = m.NoHp,
                instagram = m.Instagram,
                univ = m.NamaPerguruanTinggi,
                nim = m.NIM,
                jurusan = m.Jurusan,
                fakultas = m.Fakultas,
                company = m.Company,
                lokasi = m.Region,
                tempatLahir = m.TempatLahir,
                tglLahir = m.TanggalLahir.ToString("dd MMMM yyyy"),
                createdAtFormatted = m.CreatedAt.ToString("dd MMMM yyyy"),
                tglMulai = m.MulaiMagang.ToString("dd MMM yyyy"),
                tglSelesai = m.SelesaiMagang.ToString("dd MMM yyyy"),
                rekomendasi = m.RekomendasiPegawai ?? "Tidak Ada",
                fotoProfil = string.IsNullOrEmpty(m.FotoProfil) ? "/img/default-user.png" : "/" + m.FotoProfil,
                pathCV = "/" + m.FileCv,
                pathSurat = "/" + m.FileSuratPengantar,
                pathProposal = "/" + m.FileProposal,
                status = m.Status,

                // ✅ TAMBAHAN
                revisiFields  = m.RevisiFields ?? "",   // "CV, Surat Pengantar, Data Akademik"
                catatanRevisi = m.CatatanRevisi ?? ""   // pesan dari admin
            });
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Store(
            Magang model,
            IFormFile? Foto,
            IFormFile? FileCV,
            IFormFile? FileSuratPengantar,
            IFormFile? FileProposal)
        {
            ModelState.Clear();

            try
            {
                string currentUserEmail = User.Identity?.Name ?? "";
                    if (string.IsNullOrEmpty(currentUserEmail))
                    {
                        TempData["ErrorMessage"] = "Sesi habis, silakan login ulang.";
                        return RedirectToAction("Login", "Auth");
                    }
                model.EmailPribadi = currentUserEmail;

                // ===== 1. UPLOAD FILE =====
                model.FotoProfil = SimpanFile(Foto, "uploads/foto");
                model.FileCv = SimpanFile(FileCV, "uploads/cv");
                model.FileSuratPengantar = SimpanFile(FileSuratPengantar, "uploads/surat");
                model.FileProposal = SimpanFile(FileProposal, "uploads/proposal");

                model.CreatedAt = DateTime.Now;
                model.Status = "Menunggu";

                // === TAMBAHKAN LOGIKA NORMALISASI NAMA KAMPUS DI SINI ===
                if (!string.IsNullOrEmpty(model.NamaPerguruanTinggi))
                {
                    // 1. Bersihkan spasi depan/belakang dan ubah ke kecil semua
                    string input = model.NamaPerguruanTinggi.Trim().ToLower();

                    // 2. Ubah jadi Title Case (Contoh: universitas diponegoro -> Universitas Diponegoro)
                    var textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
                    model.NamaPerguruanTinggi = textInfo.ToTitleCase(input);

                    // 3. Mapping Otomatis jika user masih bandel ngetik singkatan
                    var mapping = new Dictionary<string, string>
                    {
                        { "Undip", "Universitas Diponegoro" },
                        { "Ugm", "Universitas Gadjah Mada" },
                        { "Ui", "Universitas Indonesia" },
                        { "Itb", "Institut Teknologi Bandung" },
                        { "Unair", "Universitas Airlangga" },
                        { "Uns", "Universitas Sebelas Maret" },
                        { "Its", "Institut Teknologi Sepuluh Nopember" },
                        { "Ub", "Universitas Brawijaya" },
                        { "Unpad", "Universitas Padjadjaran" }
                    };

                    if (mapping.ContainsKey(model.NamaPerguruanTinggi))
                    {
                        model.NamaPerguruanTinggi = mapping[model.NamaPerguruanTinggi];
                    }
                }

                // Hapus draft lama milik user ini
                var oldDraft = await _context.PendaftaranMagang
                    .FirstOrDefaultAsync(x => x.EmailPribadi == currentUserEmail && x.Status == "Draft");

                if (oldDraft != null)
                {
                    _context.PendaftaranMagang.Remove(oldDraft);
                }

                // ===== 2. SIMPAN KE DATABASE =====
                _context.PendaftaranMagang.Add(model);
                await _context.SaveChangesAsync(); 

                // Nomor pendaftaran resmi
                model.NoPendaftaran = $"PEN/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{model.Id:D4}";

                // Simpan Notifikasi
                _context.Notifications.Add(new Notification
                {
                    Nama = model.NamaLengkap,
                    UserEmail = model.EmailPribadi,
                    Title = "Pendaftaran Magang Terkirim", 
                    Message = $"Pendaftaran magang di {model.Company} ({model.Region}) berhasil dikirim. Silakan tunggu konfirmasi dari tim HC.",  // ← Update
                    Url = "/DashboardPeserta?tab=riwayat",
                    Type = "new",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    ExternalId = model.Id.ToString()
                });

                // NOTIF KE ADMIN (pendaftaran baru)
                _context.AdminNotifications.Add(new AdminNotification
                {
                    Title = "Pendaftaran Baru",
                    Message = $"{model.NamaLengkap} mendaftar di {model.Region}",
                    Type = "Baru",
                    TargetRegion = model.Region,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    MagangId = model.Id
                });

                await _context.SaveChangesAsync();

                // ===== 3. LOGIKA EMAIL (DIBUNGKUS TRY-CATCH MASING-MASING) =====
                
                // Email ke Admin
                // try {
                //     var adminUser = await _context.Admins
                //         .FirstOrDefaultAsync(a => a.RegionManaged.ToLower().Trim() == model.Region.ToLower().Trim());
                    
                //     if (adminUser != null && !string.IsNullOrEmpty(adminUser.Email)) {
                //         string subjekAdmin = "Notifikasi SINTA: Pendaftaran Magang Baru";
                //         string pesanAdmin = $"<h2>Halo, {adminUser.Nama}</h2><p>Pendaftar baru: {model.NamaLengkap} untuk wilayah {model.Region}.</p>";
                //         await _emailService.SendWithCourierAsync(adminUser.Email, subjekAdmin, pesanAdmin);
                //     }
                // } catch (Exception ex) {
                //     Console.WriteLine("DEBUG ERROR ADMIN: " + ex.Message);
                // }

                // Email ke User
                try {
                    string subjekUser = "Pendaftaran Magang Berhasil - " + model.NoPendaftaran;
                    string pesanUser = $@"
                        <div style='font-family: sans-serif; line-height: 1.6; color: #333;'>
                            <p>Yth. Sdr/i <b>{model.NamaLengkap}</b>,</p>
                            <p>Pendaftaran penelitian Anda telah masuk dalam sistem dengan nomor pendaftaran:</p>
                            <p style='font-size: 18px; color: #003399;'><b>{model.NoPendaftaran}</b></p>
                            <p>Silakan tunggu email tanggapan dari kami.</p>
                            <p>Salam hormat,<br/><b>Human Capital</b></p>
                        </div>";
                    
                    await _emailService.SendWithCourierAsync(currentUserEmail, subjekUser, pesanUser);
                } catch (Exception ex) {
                    Console.WriteLine("DEBUG ERROR USER: " + ex.Message);
                }

                // ===== 4. SELESAI & PINDAH HALAMAN =====
                TempData["Success"] = "Pendaftaran berhasil dikirim!";
                return RedirectToAction("Sukses", "PendaftaranMagang");
            }
            catch (Exception ex)
            {
                Console.WriteLine("CRITICAL ERROR: " + ex.Message);
                Console.WriteLine("STACK: " + ex.StackTrace);
                // Tampilkan langsung di browser buat debug
                return Content($"ERROR: {ex.Message} | INNER: {ex.InnerException?.Message} | STACK: {ex.StackTrace}");
            }
        }

        private string SimpanFile(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return string.Empty;
            
            try
            {
                string uploadDir = Path.Combine(_environment.WebRootPath, folder);
                if (!Directory.Exists(uploadDir)) 
                {
                    Directory.CreateDirectory(uploadDir);
                }
                
                string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string fullPath = Path.Combine(uploadDir, fileName);
                
                using (var stream = new FileStream(fullPath, FileMode.Create)) 
                { 
                    file.CopyTo(stream); 
                }
                
                Console.WriteLine($"File berhasil disimpan: {folder}/{fileName}");
                return $"{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error simpan file: {ex.Message}");
                return string.Empty;
            }
        }

        private void HapusFile(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    string fullPath = Path.Combine(_environment.WebRootPath, filePath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        Console.WriteLine($"File lama dihapus: {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hapus file: {ex.Message}");
            }
        }
        
        [AllowAnonymous]
        public IActionResult Sukses() => View();
    }
}