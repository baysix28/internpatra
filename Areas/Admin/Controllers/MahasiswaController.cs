using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;
using sinta_asp.Areas.Admin.Models;
using sinta_asp.Services;
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]
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

        // Helper untuk mengecek akses SuperAdmin atau Region Jawa Bagian Tengah
        private bool IsUserAuthorized()
        {
            var adminRole = HttpContext.Session.GetString("AdminRole")?.Trim();
            var adminRegion = HttpContext.Session.GetString("AdminRegion")?.Trim();

            bool isSuperAdmin = string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
            bool isJawaTengah = string.Equals(adminRegion, "Regional Jawa Bagian Tengah", StringComparison.OrdinalIgnoreCase);

            return isSuperAdmin || isJawaTengah;
        }

        [HttpGet]
        public async Task<IActionResult> CheckCompletion()
        {
            try
            {
                var hariIni = DateTime.Today;
                var besok = hariIni.AddDays(1);
                
                var pesertaSelesai = await _context.PendaftaranMagang
                    .Where(m => m.Status == "Diterima" && 
                                m.SelesaiMagang >= hariIni && 
                                m.SelesaiMagang < besok)
                    .ToListAsync();

                int emailTerkirim = 0;

                foreach (var item in pesertaSelesai)
                {
                    var admin = await _context.Admins.FirstOrDefaultAsync(a => 
                        a.Region != null && item.Region != null &&
                        a.Region.ToLower().Trim() == item.Region.ToLower().Trim());

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

                return Json(new { 
                    success = true, 
                    message = $"{pesertaSelesai.Count} peserta selesai hari ini. {emailTerkirim} email notifikasi dikirim ke Admin." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal memproses pengecekan: " + ex.Message });
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
        public async Task<IActionResult> Index(string selectedRegion = "all")
        {
            var adminNama          = HttpContext.Session.GetString("AdminNama");
            var adminRole          = HttpContext.Session.GetString("AdminRole");
            var adminRegionManaged = HttpContext.Session.GetString("AdminRegion");

            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var allRegionsInDb = await _context.Admins
                .Where(a => !string.IsNullOrEmpty(a.Region) && a.Region.ToLower() != "all")
                .Select(a => a.Region.Trim())
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            ViewBag.AllRegions = allRegionsInDb;

            // ── Query Magang ──────────────────────────────────────────────────────
            var magangQuery = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (!IsUserAuthorized())
            {
                // Admin biasa: locked ke regionnya sendiri
                if (!string.IsNullOrEmpty(adminRegionManaged))
                {
                    magangQuery = magangQuery.Where(x => x.Region == adminRegionManaged);
                    ViewBag.SelectedRegion = adminRegionManaged;
                }
            }
            else
            {
                // SuperAdmin & Jawa Tengah: bisa filter by region
                if (selectedRegion != "all" && !string.IsNullOrEmpty(selectedRegion))
                {
                    magangQuery = magangQuery.Where(x => x.Region == selectedRegion);
                    ViewBag.SelectedRegion = selectedRegion;
                }
                else
                {
                    // Jawa Tengah: default ke regionnya sendiri jika tidak ada param
                    bool isJawaTengah = string.Equals(adminRegionManaged, "Regional Jawa Bagian Tengah", 
                                                    StringComparison.OrdinalIgnoreCase);
                    if (isJawaTengah && adminRole != "SuperAdmin")
                    {
                        magangQuery = magangQuery.Where(x => x.Region == adminRegionManaged);
                        ViewBag.SelectedRegion = adminRegionManaged;
                    }
                    else
                    {
                        ViewBag.SelectedRegion = "Semua Region";
                    }
                }
            }

            var magang = await magangQuery.OrderByDescending(m => m.CreatedAt).ToListAsync();

            // ── Query Penelitian ──────────────────────────────────────────────────
            var penelitianQuery = _context.Pendaftarans.AsNoTracking().AsQueryable();

            if (!IsUserAuthorized())
            {
                if (!string.IsNullOrEmpty(adminRegionManaged))
                    penelitianQuery = penelitianQuery.Where(x => x.Region == adminRegionManaged);
            }
            else
            {
                if (selectedRegion != "all" && !string.IsNullOrEmpty(selectedRegion))
                {
                    penelitianQuery = penelitianQuery.Where(x => x.Region == selectedRegion);
                }
                else
                {
                    bool isJawaTengah = string.Equals(adminRegionManaged, "Regional Jawa Bagian Tengah",
                                                    StringComparison.OrdinalIgnoreCase);
                    if (isJawaTengah && adminRole != "SuperAdmin")
                        penelitianQuery = penelitianQuery.Where(x => x.Region == adminRegionManaged);
                }
            }

            var penelitianRaw = await penelitianQuery.OrderByDescending(p => p.CreatedAt).ToListAsync();

            // Normalisasi status: "Dalam Proses" → tampil sebagai "Menunggu"
            foreach (var p in penelitianRaw)
            {
                if (p.Status == "Dalam Proses")
                    p.Status = "Menunggu";
            }

            var viewModel = new DashboardModel
            {
                AdminName    = adminNama,
                LoginTime    = DateTime.Now,

                // Magang
                DaftarMagang     = magang,
                StatusDiproses   = magang.Count(x => x.Status == "Menunggu" || x.Status == "Proses Review"),
                StatusDiterima   = magang.Count(x => x.Status == "Diterima"),
                StatusDitolak    = magang.Count(x => x.Status == "Ditolak"),
                TotalInternAktif = magang.Count(x => x.Status == "Diterima"),

                // Penelitian
                DaftarPenelitian  = penelitianRaw,
                PenStatusDiproses = penelitianRaw.Count(x => x.Status == "Menunggu"),
                PenStatusDiterima = penelitianRaw.Count(x => x.Status == "Diterima"),
                PenStatusDitolak  = penelitianRaw.Count(x => x.Status == "Ditolak"),
            };

            ViewBag.AdminRole         = adminRole;
            ViewBag.RawSelectedRegion = selectedRegion;

            return View(viewModel);
        }
        private Task SimpanNotifikasiPeserta(Magang mhs, string status)
        {
            string title = "";
            string message = "";
            string type = "";

            if (status == "Diterima")
            {
                title = "Selamat! Lamaran Diterima";
                message = $"Selamat {mhs.NamaLengkap}! Pengajuan magang kamu di {mhs.Company} ({mhs.Region}) telah DITERIMA.";
                type = "success";
            }
            else if (status == "Ditolak")
            {
                title = "Pengajuan Ditolak";
                message = $"Mohon maaf {mhs.NamaLengkap}, pengajuan magang di {mhs.Company} ({mhs.Region}) tidak dapat kami terima saat ini.";
                type = "error";
            }
            else
            {
                title = "Update Status Magang";
                message = $"Status pengajuan magang kamu di {mhs.Company} diperbarui menjadi: {status}.";
                type = "info";
            }

            var notif = new Notification
            {
                Nama = mhs.NamaLengkap,
                Lokasi = mhs.Region,
                Type = type,
                UserEmail = mhs.EmailPribadi,
                Title = title,
                Message = message,
                Url = "/DashboardPeserta#riwayat",
                CreatedAt = DateTime.Now,
                IsRead = false,
                ExternalId = mhs.Id.ToString()
            };

            _context.Notifications.Add(notif);
            return Task.CompletedTask;
        }

        private Task SimpanNotifikasiAdmin(Magang mhs, string status)
        {
            string title = status == "Diterima" ? "Peserta Magang Diterima" : 
                           status == "Ditolak" ? "Peserta Magang Ditolak" : 
                           status == "Revisi" ? "Permintaan Revisi" : "Update Status";
            
            string message = $"{mhs.NamaLengkap} statusnya diperbarui menjadi {status} di {mhs.Region}.";

            var notif = new AdminNotification
            {
                Title = title,
                Message = message,
                Type = status,
                TargetRegion = mhs.Region,
                CreatedAt = DateTime.Now,
                IsRead = false,
                MagangId = mhs.Id
            };

            _context.AdminNotifications.Add(notif);
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string catatan = "")
        {
            if (!IsUserAuthorized())
            {
                return Json(new { success = false, message = "Akses Ditolak: Anda tidak memiliki hak akses untuk mengedit data." });
            }

            try
            {
                var data = await _context.PendaftaranMagang.FindAsync(id);
                if (data == null)
                    return Json(new { success = false, message = "Data mahasiswa tidak ditemukan." });

                data.Status = status;

                _context.PendaftaranMagang.Update(data);

                await SimpanNotifikasiPeserta(data, status);
                await SimpanNotifikasiAdmin(data, status);
                await _context.SaveChangesAsync();

                string emailInfo = "";
                try
                {
                    await KirimEmailNotifikasi(data, status, catatan);
                    emailInfo = "dan email notifikasi berhasil dikirim.";
                }
                catch (Exception exEmail)
                {
                    emailInfo = $"namun email gagal dikirim: {exEmail.Message}";
                }

                return Json(new { success = true, message = $"Status diperbarui {emailInfo}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatusPenelitian(int id, string status, string catatan = "")
        {
            if (!IsUserAuthorized())
                return Json(new { success = false, message = "Akses Ditolak." });

            try
            {
                var data = await _context.Pendaftarans.FindAsync(id);
                if (data == null)
                    return Json(new { success = false, message = "Data tidak ditemukan." });

                // Simpan ke DB dengan status asli yang benar
                data.Status = status;
                _context.Pendaftarans.Update(data);

                // Notifikasi peserta
                var notif = new Notification
                {
                    Nama      = data.Nama,
                    Lokasi    = data.Region,
                    Type      = status == "Diterima" ? "success" : status == "Ditolak" ? "error" : "info",
                    UserEmail = data.Email,
                    Title     = status == "Diterima" ? "Selamat! Penelitian Diterima" : "Informasi Status Penelitian",
                    Message   = status == "Diterima"
                        ? $"Selamat {data.Nama}! Pengajuan penelitian kamu di {data.Region} telah DITERIMA."
                        : $"Mohon maaf {data.Nama}, pengajuan penelitian di {data.Region} tidak dapat kami terima saat ini.",
                    Url       = "/DashboardPeserta#riwayat",
                    CreatedAt = DateTime.Now,
                    IsRead    = false,
                    ExternalId = data.Id.ToString()
                };
                _context.Notifications.Add(notif);

                // Notifikasi admin
                var notifAdmin = new AdminNotification
                {
                    Title        = status == "Diterima" ? "Peserta Penelitian Diterima" : "Peserta Penelitian Ditolak",
                    Message      = $"{data.Nama} statusnya diperbarui menjadi {status} di {data.Region}.",
                    Type         = status,
                    TargetRegion = data.Region,
                    CreatedAt    = DateTime.Now,
                    IsRead       = false,
                    MagangId     = data.Id
                };
                _context.AdminNotifications.Add(notifAdmin);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Status penelitian berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        private async Task KirimEmailNotifikasi(Magang mhs, string status, string catatanRevisi = "")
        {
            var validStatuses = new[] { "Diterima", "Ditolak", "Revisi" };
            if (!validStatuses.Contains(status)) return;

            var admin = await _context.Admins.FirstOrDefaultAsync(a => 
                a.Region != null && mhs.Region != null &&
                a.Region.ToLower().Trim() == mhs.Region.ToLower().Trim());

            string root = _env.WebRootPath;
            string templateFileName = status == "Diterima" ? "EmailDiterima.html" : 
                                      status == "Ditolak" ? "EmailDitolak.html" : "EmailRevisi.html";
            string rawFileName = status == "Diterima" ? "Diterima.txt" : 
                                status == "Ditolak" ? "Ditolak.txt" : "Revisi.txt";

            string pathHtml = Path.Combine(root, "templates", templateFileName);
            string pathTxt = Path.Combine(root, "templates", "raw", rawFileName);

            if (!System.IO.File.Exists(pathHtml) || !System.IO.File.Exists(pathTxt)) return;

            var culture = new CultureInfo("id-ID");
            string isiPesan = await System.IO.File.ReadAllTextAsync(pathTxt);
            
            isiPesan = isiPesan.Replace("{Nama}", mhs.NamaLengkap)
                               .Replace("{Region}", mhs.Region)
                               .Replace("{Unit}", mhs.Jurusan ?? "-")
                               .Replace("{TanggalMulai}", mhs.MulaiMagang.ToString("dd MMMM yyyy", culture))
                               .Replace("{TanggalSelesai}", mhs.SelesaiMagang.ToString("dd MMMM yyyy", culture));

            if (status == "Revisi") isiPesan = isiPesan.Replace("{KomentarRevisi}", catatanRevisi);

            string htmlBody = await System.IO.File.ReadAllTextAsync(pathHtml);
            htmlBody = htmlBody.Replace("{IsiPesan}", isiPesan)
                               .Replace("{Tahun}", DateTime.Now.Year.ToString());

            string subject = status == "Diterima" ? "Selamat! Seleksi Magang Diterima" : 
                             status == "Revisi" ? "Instruksi Revisi Data Pendaftaran" : "Informasi Seleksi Magang";

            await _emailService.SendWithCourierAsync(mhs.EmailPribadi, subject, htmlBody, "HC Pertamina - " + (admin?.Region ?? mhs.Region));
        }

        [HttpGet]
        public async Task<IActionResult> ExportMahasiswa(string selectedRegion = "all")
        {
            var adminRegionManaged = HttpContext.Session.GetString("AdminRegion");
            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (!IsUserAuthorized())
            {
                if (!string.IsNullOrEmpty(adminRegionManaged))
                {
                    query = query.Where(x => x.Region == adminRegionManaged);
                }
            }
            else
            {
                if (selectedRegion != "all" && !string.IsNullOrEmpty(selectedRegion))
                {
                    query = query.Where(x => x.Region == selectedRegion);
                }
            }

            var data = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data Mahasiswa");
                string[] headers = { "ID", "Tgl Daftar", "NIM", "Nama Lengkap", "Email", "No HP", "Instagram", "Tempat Lahir", "Tgl Lahir", "Universitas", "Fakultas", "Jurusan", "Company", "Region", "Lokasi Unit", "Rekomendasi", "Mulai", "Selesai", "Status", "Link CV", "Link Surat", "Link Proposal", "Link Foto" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#00549B");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.CreatedAt.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 3).Value = item.NIM;
                    worksheet.Cell(row, 4).Value = item.NamaLengkap;
                    worksheet.Cell(row, 5).Value = item.EmailPribadi;
                    worksheet.Cell(row, 6).Value = item.NoHp;
                    worksheet.Cell(row, 7).Value = item.Instagram ?? "-";
                    worksheet.Cell(row, 8).Value = item.TempatLahir;
                    worksheet.Cell(row, 9).Value = item.TanggalLahir.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 10).Value = item.NamaPerguruanTinggi;
                    worksheet.Cell(row, 11).Value = item.Fakultas;
                    worksheet.Cell(row, 12).Value = item.Jurusan;
                    worksheet.Cell(row, 13).Value = item.Company;
                    worksheet.Cell(row, 14).Value = item.Region;
                    worksheet.Cell(row, 15).Value = item.Lokasi;
                    worksheet.Cell(row, 16).Value = item.RekomendasiPegawai ?? "-";
                    worksheet.Cell(row, 17).Value = item.MulaiMagang.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 18).Value = item.SelesaiMagang.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 19).Value = item.Status;

                    AddHyperlink(worksheet.Cell(row, 20), item.FileCv, "cv", baseUrl);
                    AddHyperlink(worksheet.Cell(row, 21), item.FileSuratPengantar, "surat", baseUrl);
                    AddHyperlink(worksheet.Cell(row, 22), item.FileProposal, "proposal", baseUrl);
                    AddHyperlink(worksheet.Cell(row, 23), item.FotoProfil, "foto", baseUrl);
                    row++;
                }
                worksheet.Columns().AdjustToContents();
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                var regionLabel = (selectedRegion == "all" || string.IsNullOrEmpty(selectedRegion)) ? "Semua_Region" : selectedRegion;
                if (!IsUserAuthorized()) regionLabel = adminRegionManaged ?? "Region";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Rekap_Magang_{regionLabel}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        private void AddHyperlink(IXLCell cell, string? fileName, string folder, string baseUrl)
        {
            if (string.IsNullOrEmpty(fileName)) cell.Value = "-";
            else
            {
                var fullPath = $"{baseUrl}/uploads/{folder}/{Path.GetFileName(fileName)}";
                cell.Value = "Lihat Dokumen";
                cell.GetHyperlink().ExternalAddress = new Uri(fullPath);
                cell.Style.Font.FontColor = XLColor.Blue;
                cell.Style.Font.Underline = XLFontUnderlineValues.Single;
            }
        }
    }
}