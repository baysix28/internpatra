using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Areas.Admin.Models;
using System.Net;
using System.Net.Mail;
using System.Globalization;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MahasiswaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MahasiswaController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            // --- FILTER LOGIC (Sama dengan Dashboard) ---
            var query = _context.PendaftaranMagang.AsQueryable();

            if (adminNama == "Admin MOR I") query = query.Where(x => x.Region == "Regional Sumbagut");
            else if (adminNama == "Admin MOR III") query = query.Where(x => x.Region == "Regional Jawa Bagian Barat");
            else if (adminNama == "Admin MOR IV") query = query.Where(x => x.Region == "Regional Jawa Bagian Tengah");
            else if (adminNama == "Admin MOR V") query = query.Where(x => x.Region == "Regional Jatimbalinus");
            else if (adminNama == "Admin MOR VI") query = query.Where(x => x.Region == "Regional Kalimantan");
            else if (adminNama == "Admin MOR VIII") query = query.Where(x => x.Region == "Regional Maluku Papua");
            else if (adminNama == "Admin RU VI") query = query.Where(x => x.Region == "Refinery Unit VI Balongan");

            var magang = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();

            var viewModel = new DashboardModel
            {
                AdminName = adminNama,
                LoginTime = DateTime.Now,
                DaftarMagang = magang,
                StatusDiproses = magang.Count(x => x.Status == "Menunggu"),
                StatusDiterima = magang.Count(x => x.Status == "Diterima"),
                StatusDitolak = magang.Count(x => x.Status == "Ditolak"),
                TotalInternAktif = magang.Count(x => x.Status == "Diterima")
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try 
            {
                var data = await _context.PendaftaranMagang.FindAsync(id);
                if (data == null) 
                    return Json(new { success = false, message = "Data tidak ditemukan." });

                if (data.Status == status)
                    return Json(new { success = true, message = "Status tidak berubah." });

                data.Status = status;
                await _context.SaveChangesAsync();

                // Kirim email hanya jika status Diterima/Ditolak
                if (status == "Diterima" || status == "Ditolak")
                {
                    // Jangan pakai 'await' di sini jika ingin proses UI lebih cepat 
                    // (Fire and Forget) atau tetap pakai await agar yakin terkirim.
                    await KirimEmailNotifikasi(data, status);
                }
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private async Task KirimEmailNotifikasi(dynamic mhs, string status)
        {
            try
            {
                string fileName = status == "Diterima" ? "EmailDiterima.html" : "EmailDitolak.html";
                string pathToFile = Path.Combine(_env.WebRootPath, "templates", fileName);

                string body = "";
                if (System.IO.File.Exists(pathToFile))
                {
                    body = await System.IO.File.ReadAllTextAsync(pathToFile);
                }
                else
                {
                    body = $"<p>Halo {mhs.NamaLengkap}, lamaran Anda dinyatakan: <b>{status}</b></p>";
                }

                var cultureInfo = new CultureInfo("id-ID");
                string tglMulai = mhs.MulaiMagang != null ? ((DateTime)mhs.MulaiMagang).ToString("dd MMMM yyyy", cultureInfo) : "-";
                string tglSelesai = mhs.SelesaiMagang != null ? ((DateTime)mhs.SelesaiMagang).ToString("dd MMMM yyyy", cultureInfo) : "-";

                // Replace Placeholder
                body = body.Replace("{Nama}", mhs.NamaLengkap)
                           .Replace("{Unit}", mhs.UnitKerja ?? mhs.Jurusan ?? "-") // Mengambil Unit atau Jurusan
                           .Replace("{Lokasi}", mhs.Region ?? "-")
                           .Replace("{TanggalMulai}", tglMulai)
                           .Replace("{TanggalSelesai}", tglSelesai)
                           .Replace("{Tahun}", DateTime.Now.Year.ToString());

                // --- KONFIGURASI SMTP ---
                string senderEmail = "email-anda@gmail.com"; 
                string senderPass = "xxxx xxxx xxxx xxxx"; // Masukkan App Password Anda

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(senderEmail, "Pertamina Internship");
                    message.To.Add(new MailAddress(mhs.EmailPribadi));
                    message.Subject = status == "Diterima" ? "Selamat! Anda Diterima Magang" : "Informasi Seleksi Magang";
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(senderEmail, senderPass);
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error ke Console agar bisa dicek di Output Visual Studio
                System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
            }
        }
    }
}