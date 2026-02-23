using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authorization;
using sinta_asp.Services;
using sinta_asp.Areas.Admin.Models; // Tambahkan namespace model admin notification

namespace sinta_asp.Controllers
{
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

        public IActionResult Index() => View();

        [Authorize]
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
                status = m.Status
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                string currentUserEmail = User.Identity?.Name ?? "guest@gmail.com";
                model.EmailPribadi = currentUserEmail;

                // ===== UPLOAD FILE =====
                model.FotoProfil = SimpanFile(Foto, "uploads/foto");
                model.FileCv = SimpanFile(FileCV, "uploads/cv");
                model.FileSuratPengantar = SimpanFile(FileSuratPengantar, "uploads/surat");
                model.FileProposal = SimpanFile(FileProposal, "uploads/proposal");

                model.CreatedAt = DateTime.Now;
                model.Status = "Menunggu";

                // Hapus draft lama milik user ini
                var oldDraft = await _context.PendaftaranMagang
                    .FirstOrDefaultAsync(x => x.EmailPribadi == currentUserEmail && x.Status == "Draft");

                if (oldDraft != null)
                {
                    _context.PendaftaranMagang.Remove(oldDraft);
                }

                // ===== SIMPAN KE DATABASE =====
                _context.PendaftaranMagang.Add(model);

                await _context.SaveChangesAsync(); 
                // penting: supaya model.Id sudah terbentuk

                // ===== NOMOR PENDAFTARAN RESMI (BERDASARKAN ID) =====
                model.NoPendaftaran = $"PEN/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{model.Id:D4}";

                // ===== NOTIFIKASI KE USER (Tabel Peserta) =====
                _context.Notifications.Add(new Notification
                {
                    Nama = model.NamaLengkap,
                    UserEmail = model.EmailPribadi,
                    Title = "Pendaftaran Magang",
                    Message = $"Pendaftaran magang di {model.Company} berhasil dikirim.",
                    Url = "/DashboardPeserta#riwayat",
                    Type = "new",   
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    ExternalId = model.Id.ToString()
                });

                // ===== NOTIFIKASI KE ADMIN (Tabel AdminNotifications) =====
                _context.AdminNotifications.Add(new AdminNotification
                {
                    MagangId = model.Id,
                    TargetRegion = model.Region, // Agar admin region tertentu bisa melihat
                    Title = "Pendaftaran Baru",
                    Message = $"{model.NamaLengkap} mendaftar di {model.Lokasi} ({model.Region})",
                    Type = "Baru",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();

                // ===== LOGIKA EMAIL 1: KE ADMIN =====
                var adminUser = await _context.Admins
                    .FirstOrDefaultAsync(a => a.RegionManaged.ToLower().Trim() == model.Region.ToLower().Trim());

                if (adminUser != null && !string.IsNullOrEmpty(adminUser.Email))
                {
                    try 
                    {
                        string subjekAdmin = "Notifikasi SINTA: Pendaftaran Magang Baru";
                        string pesanAdmin = $"<h2>Halo, {adminUser.Nama}</h2><p>Pendaftar baru: {model.NamaLengkap} untuk wilayah {model.Region}.</p>";
                        await _emailService.SendWithCourierAsync(adminUser.Email, subjekAdmin, pesanAdmin);
                    }
                    catch (Exception ex) {
                        Console.WriteLine("DEBUG ERROR ADMIN: " + ex.Message);
                    }
                }

                // ===== LOGIKA EMAIL 2: KE KAMU (USER) =====
                try 
                {
                    string subjekUser = "Pendaftaran Magang Berhasil - " + model.NoPendaftaran;
                    string pesanUser = $@"
                        <div style='font-family: sans-serif; line-height: 1.6; color: #333;'>
                            <p>Yth. Sdr/i <b>{model.NamaLengkap}</b>,</p>
                            <p>Pendaftaran penelitian Anda telah masuk dalam sistem dengan nomor pendaftaran:</p>
                            <p style='font-size: 18px; color: #003399;'><b>{model.NoPendaftaran}</b></p>
                            <p>Silakan tunggu email tanggapan dari kami atau periksa status penerimaan penelitian Anda melalui Web Sinta dengan memasukkan nomor pendaftaran tersebut.</p>
                            <p>
                                Salam hormat,<br/>
                                <b>Human Capital</b><br/>
                                PT Pertamina Patra Niaga Regional Jawa Bagian Tengah
                            </p>
                            <hr style='border: none; border-top: 1px solid #eee; margin-top: 20px;'/>
                            <p style='font-size: 11px; color: gray;'>*Email ini dikirimkan secara otomatis, mohon untuk <b>tidak membalas (do not reply)</b> email ini.</p>
                        </div>";

                    await _emailService.SendWithCourierAsync(currentUserEmail, subjekUser, pesanUser);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("DEBUG ERROR USER: " + ex.Message);
                }

                return RedirectToAction(nameof(Sukses));
            }
            catch (Exception ex)
            {
                Console.WriteLine("CRITICAL ERROR: " + ex.Message);
                TempData["Error"] = "Gagal simpan data: " + ex.Message;
                return View("DataMagang", model); 
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

        public IActionResult Sukses() => View();
    }
}