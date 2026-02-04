using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;
using sinta_asp.Areas.Admin.Models;
using sinta_asp.Services;
using System.Globalization;
using ClosedXML.Excel;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MahasiswaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public MahasiswaController(AppDbContext context, IWebHostEnvironment env, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
        }

        // --- METHOD UNTUK CEK MAHASISWA SELESAI HARI INI ---
        [HttpGet]
        public async Task<IActionResult> CheckCompletion()
        {
            try
            {
                var hariIni = DateTime.Today;
                
                var pesertaSelesai = await _context.PendaftaranMagang
                    .Where(m => m.Status == "Diterima" && m.SelesaiMagang.Date == hariIni)
                    .ToListAsync();

                int emailTerkirim = 0;

                foreach (var item in pesertaSelesai)
                {
                    // Simpan ke tabel Notifikasi agar muncul di lonceng (Type: expired)
                    var existingNotif = await _context.Notifications
                        .AnyAsync(n => n.ExternalId == item.Id.ToString() && n.Type == "expired");

                    if (!existingNotif)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            Nama = item.NamaLengkap,
                            Lokasi = item.Region,
                            Type = "expired",
                            IsRead = false, 
                            CreatedAt = DateTime.Now,
                            ExternalId = item.Id.ToString()
                        });
                    }

                    var admin = await _context.Admins.FirstOrDefaultAsync(a => 
                        a.RegionManaged.ToLower().Trim() == item.Region.ToLower().Trim());

                    if (admin != null)
                    {
                        await _emailService.SendCompletionNotificationToAdminAsync(
                            admin.Email, 
                            item.NamaLengkap, 
                            item.Region
                        );
                        emailTerkirim++;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"{pesertaSelesai.Count} peserta selesai hari ini. {emailTerkirim} email notifikasi dikirim." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal memproses notifikasi: " + ex.Message });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var mahasiswa = await _context.PendaftaranMagang.FirstOrDefaultAsync(m => m.Id == id);
            if (mahasiswa == null) return NotFound();

            return View(mahasiswa);
        }

        public async Task<IActionResult> Index()
        {
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var query = GetFilteredQuery(adminNama);
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
                    return Json(new { success = false, message = "Data mahasiswa tidak ditemukan." });

                if (data.Status == status)
                    return Json(new { success = true, message = "Status sudah sesuai." });

                data.Status = status;

                // Tambahkan notifikasi update status
                _context.Notifications.Add(new Notification
                {
                    Nama = data.NamaLengkap,
                    Lokasi = data.Region,
                    Type = "status_update",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    ExternalId = data.Id.ToString()
                });

                await _context.SaveChangesAsync();

                if (status == "Diterima" || status == "Ditolak")
                {
                    try 
                    {
                        await KirimEmailNotifikasi(data, status);
                    }
                    catch (Exception emailEx)
                    {
                        return Json(new { 
                            success = true, 
                            message = "Status terupdate, tapi EMAIL GAGAL: " + emailEx.Message 
                        });
                    }
                }

                return Json(new { success = true, message = "Status diperbarui, notifikasi dibuat, dan email terkirim." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        // Logic baru untuk menangkap pendaftar baru hari ini (Biasanya dipanggil saat submit form pendaftaran)
        // Namun sebagai pengaman, logic notifikasi "new" biasanya diletakkan di DashboardController 
        // atau saat entitas Magang pertama kali dibuat.

        private async Task KirimEmailNotifikasi(Magang mhs, string status)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => 
                a.RegionManaged.ToLower().Trim() == mhs.Region.ToLower().Trim());

            if (admin == null) 
                throw new Exception($"Admin untuk region '{mhs.Region}' tidak ditemukan.");

            string root = _env.WebRootPath;
            string pathHtml = Path.Combine(root, "templates", status == "Diterima" ? "EmailDiterima.html" : "EmailDitolak.html");
            string pathTxt = Path.Combine(root, "templates", "raw", status == "Diterima" ? "Diterima.txt" : "Ditolak.txt");

            if (!System.IO.File.Exists(pathHtml) || !System.IO.File.Exists(pathTxt))
                throw new Exception("File template email (HTML/TXT) tidak ditemukan.");

            var culture = new CultureInfo("id-ID");
            string isiPesan = await System.IO.File.ReadAllTextAsync(pathTxt);
            isiPesan = isiPesan.Replace("{Nama}", mhs.NamaLengkap)
                               .Replace("{Region}", mhs.Region)
                               .Replace("{Unit}", mhs.Jurusan ?? "-")
                               .Replace("{TanggalMulai}", mhs.MulaiMagang.ToString("dd MMMM yyyy", culture))
                               .Replace("{TanggalSelesai}", mhs.SelesaiMagang.ToString("dd MMMM yyyy", culture));

            string htmlBody = await System.IO.File.ReadAllTextAsync(pathHtml);
            htmlBody = htmlBody.Replace("{IsiPesan}", isiPesan)
                               .Replace("{Tahun}", DateTime.Now.Year.ToString());

            await _emailService.SendAsAdminAsync(
                admin.Email, 
                admin.SmtpPassword, 
                mhs.EmailPribadi, 
                status == "Diterima" ? "Selamat! Seleksi Magang Diterima" : "Informasi Seleksi Magang",
                htmlBody,
                "HC Pertamina - " + admin.RegionManaged
            );
        }

        private IQueryable<Magang> GetFilteredQuery(string adminNama)
        {
            var query = _context.PendaftaranMagang.AsQueryable();
            var regionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Admin MOR I", "Regional Sumbagut" },
                { "Admin MOR III", "Regional Jawa Bagian Barat" },
                { "Admin MOR IV", "Regional Jawa Bagian Tengah" },
                { "Admin MOR V", "Regional Jatimbalinus" },
                { "Admin MOR VI", "Regional Kalimantan" },
                { "Admin MOR VIII", "Regional Maluku Papua" },
                { "Admin RU VI", "Refinery Unit VI Balongan" }
            };

            if (regionMap.ContainsKey(adminNama))
            {
                var targetRegion = regionMap[adminNama];
                query = query.Where(x => x.Region == targetRegion);
            }
            return query;
        }

        [HttpGet]
        public async Task<IActionResult> ExportMahasiswa()
        {
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama)) return Unauthorized();
            var data = await GetFilteredQuery(adminNama).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Daftar Mahasiswa");
                string[] headers = { "NIM", "Nama Lengkap", "Email", "No HP", "Universitas", "Fakultas", "Jurusan", "Company", "Lokasi", "Mulai Magang", "Selesai Magang", "Status" };
                for (int i = 0; i < headers.Length; i++) worksheet.Cell(1, i + 1).Value = headers[i];

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.NIM;
                    worksheet.Cell(row, 2).Value = item.NamaLengkap;
                    worksheet.Cell(row, 3).Value = item.EmailPribadi;
                    worksheet.Cell(row, 10).Value = item.MulaiMagang.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 11).Value = item.SelesaiMagang.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 12).Value = item.Status;
                    row++;
                }
                
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Data_Mahasiswa_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }
    }
}