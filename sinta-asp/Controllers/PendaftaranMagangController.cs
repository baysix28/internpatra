using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authorization;

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

        // ================= FORM UTAMA =================
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(
            Magang model,
            IFormFile? Foto,
            IFormFile? FileCV,
            IFormFile? FileSuratPengantar,
            IFormFile? FileProposal)
        {
            // Validasi Server-Side
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
                model.Status = "Menunggu";

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

                // 3. LOGIKA EMAIL KURIR (Sistem) -> ADMIN REGION
                // Mencari admin berdasarkan region yang dipilih pendaftar
                var adminUser = await _context.Admins
                    .FirstOrDefaultAsync(a => a.RegionManaged.ToLower().Trim() == model.Region.ToLower().Trim());

                if (adminUser != null && !string.IsNullOrEmpty(adminUser.Email))
                {
                    try 
                    {
                        string subjek = "Notifikasi SINTA: Pendaftaran Magang Baru";
                        
                        // Format pesan HTML untuk email kurir pusat
                        string pesan = $@"
                            <div style='font-family: sans-serif; line-height: 1.6; color: #333;'>
                                <h2>Halo, {adminUser.Nama}</h2>
                                <p>Terdapat pendaftar magang baru yang masuk ke sistem SINTA untuk wilayah <b>{model.Region}</b>.</p>
                                <table style='width: 100%; border-collapse: collapse;'>
                                    <tr><td style='width: 150px;'><b>Nama</b></td><td>: {model.NamaLengkap}</td></tr>
                                    <tr><td><b>Universitas</b></td><td>: {model.NamaPerguruanTinggi}</td></tr>
                                    <tr><td><b>Jurusan</b></td><td>: {model.Jurusan}</td></tr>
                                    <tr><td><b>Lokasi Tugas</b></td><td>: {model.Lokasi}</td></tr>
                                    <tr><td><b>Periode</b></td><td>: {model.MulaiMagang:dd MMM yyyy} - {model.SelesaiMagang:dd MMM yyyy}</td></tr>
                                </table>
                                <p>Harap segera login ke Dashboard Admin untuk memeriksa dokumen pendaftar.</p>
                                <br>
                                <hr>
                                <p style='font-size: 0.8em; color: #777;'>Email ini dikirim otomatis oleh Sistem SINTA Pertamina.</p>
                            </div>";

                        // Menggunakan SendWithCourierAsync (Email dari appsettings.json)
                        await _emailService.SendWithCourierAsync(adminUser.Email, subjek, pesan);
                    }
                    catch (Exception emailEx)
                    {
                        // Jika email gagal, pendaftaran tetap sukses tapi log error dicatat
                        System.Diagnostics.Debug.WriteLine("Gagal kirim notifikasi admin: " + emailEx.Message);
                    }
                }

                return RedirectToAction("Sukses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Terjadi kesalahan sistem saat menyimpan data: " + ex.Message;
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

            // Pastikan direktori tujuan ada
            string uploadDir = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Buat nama file unik untuk menghindari penumpukan (overwrite)
            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string fullPath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Kembalikan path relatif untuk disimpan di DB
            return $"{folder}/{fileName}";
        }

        public IActionResult Sukses()
        {
            return View();
        }
    }
}