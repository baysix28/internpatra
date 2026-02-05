using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authorization;
using sinta_asp.Services;

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
        public IActionResult DataMagang() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(
            Magang model,
            IFormFile? Foto,
            IFormFile? FileCV,
            IFormFile? FileSuratPengantar,
            IFormFile? FileProposal)
        {
            if (!ModelState.IsValid) return View("Index", model);if (!ModelState.IsValid) 
{
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("MODEL ERROR: " + error.ErrorMessage);
                }
                return View("Index", model); 
            }

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

                // ===== SIMPAN KE DATABASE =====
                _context.PendaftaranMagang.Add(model);
                
                var notif = new Notification
                {
                    Title = "Pendaftaran Berhasil",
                    Message = $"Pendaftaran Magang di unit {model.Company} berhasil dikirim.",
                    Type = "Dokumen",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UserEmail = currentUserEmail
                };
                
                _context.Set<Notification>().Add(notif);
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
                    // Membuat format nomor pendaftaran PEN/2026/02/00XX
                    string noPendaftaran = $"PEN/{DateTime.Now:yyyy}/{DateTime.Now:MM}/000{new Random().Next(1, 99)}"; 
                    
                    string subjekUser = "Pendaftaran Magang Berhasil - " + noPendaftaran;
                    string pesanUser = $@"
                        <div style='font-family: sans-serif; line-height: 1.6; color: #333;'>
                            <p>Yth. Sdr/i <b>{model.NamaLengkap}</b>,</p>
                            <p>Pendaftaran penelitian Anda telah masuk dalam sistem dengan nomor pendaftaran:</p>
                            <p style='font-size: 18px; color: #003399;'><b>{noPendaftaran}</b></p>
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
                
                return RedirectToAction("Sukses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Gagal simpan data: " + ex.Message;
                return View("Index", model);
            }
        }

        private string SimpanFile(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return string.Empty;
            string uploadDir = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string fullPath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create)) { file.CopyTo(stream); }
            return $"{folder}/{fileName}";
        }

        public IActionResult Sukses() => View();
    }
}