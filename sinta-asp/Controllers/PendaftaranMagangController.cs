using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Models;
using sinta_asp.Data;
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

        // ================= FORM UTAMA =================
        public IActionResult Index()
        {
            return View();
        }

        // ================= PROSES SIMPAN DATA & NOTIFIKASI =================
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
                // 1. PROSES UPLOAD FILE
                // Menyimpan file ke wwwroot/uploads/...
                model.FotoProfil = SimpanFile(Foto, "uploads/foto");
                model.FileCv = SimpanFile(FileCV, "uploads/cv");
                model.FileSuratPengantar = SimpanFile(FileSuratPengantar, "uploads/surat");
                model.FileProposal = SimpanFile(FileProposal, "uploads/proposal");

                model.CreatedAt = DateTime.Now;
                model.Status = "Menunggu";

                // 2. SIMPAN KE DATABASE
                _context.PendaftaranMagang.Add(model);
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

        // ================= HELPER: PENYIMPANAN FILE =================
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