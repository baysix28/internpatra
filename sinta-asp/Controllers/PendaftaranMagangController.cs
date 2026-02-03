using Microsoft.AspNetCore.Mvc;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authorization;

namespace sinta_asp.Controllers
{
    public class PendaftaranMagangController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PendaftaranMagangController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ================= FORM =================
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult DataMagang()
        {
            return View();
        }

        // ================= POST SIMPAN =================
        [HttpPost]
        public async Task<IActionResult> Store(
            Magang model,
            IFormFile? Foto,
            IFormFile? FileCV,
            IFormFile? FileSuratPengantar,
            IFormFile? FileProposal)
        {
            if (!ModelState.IsValid)
            {
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

                // ===== SIMPAN PENDAFTARAN KE DATABASE =====
                _context.PendaftaranMagang.Add(model);
                
                // 🔥 TAMBAHKAN LOGIKA NOTIFIKASI DI SINI (Sebelum SaveChanges)
                var notif = new Notification
                {
                    Title = "Pendaftaran Berhasil",
                    Message = $"Pendaftaran Magang di unit {model.Company} berhasil dikirim.",
                    Type = "Dokumen",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UserEmail = currentUserEmail // Menggunakan email user yang sedang login
                };
                
                _context.Set<Notification>().Add(notif);

                // Simpan keduanya (Pendaftaran & Notifikasi) sekaligus
                await _context.SaveChangesAsync();

                return RedirectToAction("Sukses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Index", model);
            }
        }

        // // Contoh saat pendaftaran berhasil
        // // DI DALAM fungsi pendaftaran, bukan di luar
        // var notif = new Notification
        // {
        //     Title = "Pendaftaran Berhasil",
        //     Message = "Berkas pendaftaran Anda sedang ditinjau.",
        //     Type = "Dokumen",
        //     UserEmail = "user@gmail.com" // Ganti dengan email user yang login
        // };

        // _context.Set<Notification>().Add(notif);
        // await _context.SaveChangesAsync();

        // ================= HELPER UPLOAD =================
        private string SimpanFile(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            string uploadDir = Path.Combine(_environment.WebRootPath, folder);

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            file.CopyTo(stream);

            return $"{folder}/{fileName}";
        }

        // ================= SUKSES =================
        public IActionResult Sukses()
        {
            return View();
        }
    }
}