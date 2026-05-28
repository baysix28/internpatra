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

        // ─────────────────────────────────────────────────────────────
        // CHECK COMPLETION (tidak diubah)
        // ─────────────────────────────────────────────────────────────
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

                return Json(new
                {
                    success = true,
                    message = $"{pesertaSelesai.Count} peserta selesai hari ini. {emailTerkirim} email notifikasi dikirim ke Admin."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal memproses pengecekan: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DETAILS (tidak diubah)
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var mahasiswa = await _context.PendaftaranMagang.FirstOrDefaultAsync(m => m.Id == id);
            if (mahasiswa == null) return NotFound();

            return View(mahasiswa);
        }

        // ─────────────────────────────────────────────────────────────
        // INDEX — ditambah query penelitian
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string selectedRegion = "all")
        {
            var adminNama          = HttpContext.Session.GetString("AdminNama");
            var adminRole          = HttpContext.Session.GetString("AdminRole");
            var adminRegionManaged = HttpContext.Session.GetString("AdminRegion");

            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            // Region dinamis dari DB
            var allRegionsInDb = await _context.Admins
                .Where(a => !string.IsNullOrEmpty(a.Region) && a.Region.ToLower() != "all")
                .Select(a => a.Region.Trim())
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            ViewBag.AllRegions = allRegionsInDb;

            // Query Magang
            var queryMagang = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            // Query Penelitian
            // ↓ Ganti "PendaftaranPenelitians" dengan nama DbSet penelitian di AppDbContext kamu
            var queryPenelitian = _context.Pendaftarans.AsNoTracking().AsQueryable();

            // Filter berdasarkan Role
            if (!string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(adminRegionManaged))
                {
                    queryMagang     = queryMagang.Where(x => x.Region == adminRegionManaged);
                    queryPenelitian = queryPenelitian.Where(x => x.Region == adminRegionManaged);
                    ViewBag.SelectedRegion = adminRegionManaged;
                }
            }
            else
            {
                if (selectedRegion != "all" && !string.IsNullOrEmpty(selectedRegion))
                {
                    queryMagang     = queryMagang.Where(x => x.Region == selectedRegion);
                    queryPenelitian = queryPenelitian.Where(x => x.Region == selectedRegion);
                    ViewBag.SelectedRegion = selectedRegion;
                }
                else
                {
                    ViewBag.SelectedRegion = "Semua Region";
                }
            }

            var magang     = await queryMagang.OrderByDescending(m => m.CreatedAt).ToListAsync();

            // ↓ Sesuaikan field tanggal di entity penelitian kamu (CreatedAt / TglDaftar / dll)
            var penelitian = await queryPenelitian.OrderByDescending(p => p.CreatedAt).ToListAsync();

            var viewModel = new DashboardModel
            {
                AdminName        = adminNama,
                LoginTime        = DateTime.Now,
                DaftarMagang     = magang,
                StatusDiproses   = magang.Count(x => x.Status == "Menunggu" || x.Status == "Proses Review"),
                StatusDiterima   = magang.Count(x => x.Status == "Diterima"),
                StatusDitolak    = magang.Count(x => x.Status == "Ditolak"),
                TotalInternAktif = magang.Count(x => x.Status == "Diterima"),
                StatusRevisi     = magang.Count(x => x.Status == "Revisi"),

                // Penelitian
                DaftarPenelitian   = penelitian,
                PenStatusDiproses  = penelitian.Count(x => x.Status == "Dalam Proses"),
                PenStatusDiterima  = penelitian.Count(x => x.Status == "Diterima"),
                PenStatusDitolak   = penelitian.Count(x => x.Status == "Ditolak"),
            };

            ViewBag.AdminRole         = adminRole;
            ViewBag.RawSelectedRegion = selectedRegion;

            return View(viewModel);
        }

        // ─────────────────────────────────────────────────────────────
        // HELPER: Notifikasi Peserta Magang (tidak diubah)
        // ─────────────────────────────────────────────────────────────
        private Task SimpanNotifikasiPeserta(Magang mhs, string status)
        {
            string title = "";
            string message = "";
            string type = "";

            if (status == "Diterima")
            {
                title   = "Selamat! Lamaran Diterima";
                message = $"Selamat {mhs.NamaLengkap}! Pengajuan magang kamu di {mhs.Company} ({mhs.Region}) telah DITERIMA.";
                type    = "success";
            }
            else if (status == "Ditolak")
            {
                title   = "Pengajuan Ditolak";
                message = $"Mohon maaf {mhs.NamaLengkap}, pengajuan magang di {mhs.Company} ({mhs.Region}) tidak dapat kami terima saat ini.";
                type    = "error";
            }
            else if (status == "Revisi")
            {
                title   = "Instruksi Revisi Data";
                message = $"Halo {mhs.NamaLengkap}, terdapat beberapa data pendaftaran yang perlu diperbaiki. Silakan cek email Anda untuk detail instruksi.";
                type    = "warning";
            }
            else
            {
                title   = "Update Status Magang";
                message = $"Status pengajuan magang kamu di {mhs.Company} diperbarui menjadi: {status}.";
                type    = "info";
            }

            var notif = new Notification
            {
                Nama      = mhs.NamaLengkap,
                Lokasi    = mhs.Region,
                Type      = type,
                UserEmail = mhs.EmailPribadi,
                Title     = title,
                Message   = message,
                Url       = "/DashboardPeserta#riwayat",
                CreatedAt = DateTime.Now,
                IsRead    = false,
                ExternalId = mhs.Id.ToString()
            };

            _context.Notifications.Add(notif);
            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────
        // HELPER: Notifikasi Admin (tidak diubah)
        // ─────────────────────────────────────────────────────────────
        private Task SimpanNotifikasiAdmin(Magang mhs, string status)
        {
            string title = status == "Diterima" ? "Peserta Diterima" :
                           status == "Ditolak"  ? "Peserta Ditolak"  :
                           status == "Revisi"   ? "Permintaan Revisi" : "Update Status";

            string message = $"{mhs.NamaLengkap} statusnya diperbarui menjadi {status} di {mhs.Region}.";

            var notif = new AdminNotification
            {
                Title        = title,
                Message      = message,
                Type         = status,
                TargetRegion = mhs.Region,
                CreatedAt    = DateTime.Now,
                IsRead       = false,
                MagangId     = mhs.Id
            };

            _context.AdminNotifications.Add(notif);
            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────
        // UPDATE STATUS MAGANG (tidak diubah)
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string catatan = "")
        {
            var rawRole   = HttpContext.Session.GetString("AdminRole") ?? "";
            var adminRole = rawRole.Trim();

            if (string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Akses Ditolak: Akun SuperAdmin hanya memiliki hak akses Lihat Data (Read-Only)." });
            }

            try
            {
                var data = await _context.PendaftaranMagang.FindAsync(id);
                if (data == null)
                    return Json(new { success = false, message = "Data mahasiswa tidak ditemukan." });

                data.Status = status;

                if (status == "Revisi" && !string.IsNullOrEmpty(catatan))
                {
                    var parts = catatan.Split(" | Pesan: ", 2, StringSplitOptions.None);
                    data.RevisiFields  = parts[0].Trim();
                    data.CatatanRevisi = parts.Length > 1 ? parts[1].Trim() : null;
                }
                else if (status == "Menunggu")
                {
                    data.RevisiFields  = null;
                    data.CatatanRevisi = null;
                }

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
                    Console.WriteLine($"[EMAIL ERROR] UpdateStatus id={id}: {exEmail}");
                }

                return Json(new { success = true, message = $"Status diperbarui {emailInfo}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // UPDATE STATUS PENELITIAN (baru)
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateStatusPenelitian(int id, string status)
        {
            var rawRole   = HttpContext.Session.GetString("AdminRole") ?? "";
            var adminRole = rawRole.Trim();

            if (string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Akses Ditolak: Akun SuperAdmin hanya memiliki hak akses Lihat Data (Read-Only)." });
            }

            try
            {
                // ↓ Ganti "PendaftaranPenelitians" dengan nama DbSet penelitian di AppDbContext kamu
                var data = await _context.Pendaftarans.FindAsync(id);
                if (data == null)
                    return Json(new { success = false, message = "Data penelitian tidak ditemukan." });

                data.Status = status;
                _context.Update(data);

                // Simpan notifikasi untuk peserta penelitian
                await SimpanNotifikasiPesertaPenelitian(data, status);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Status penelitian diperbarui menjadi {status}." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // HELPER: Notifikasi Peserta Penelitian (baru)
        // ─────────────────────────────────────────────────────────────
        private Task SimpanNotifikasiPesertaPenelitian(Pendaftaran p, string status)
        // ↑ Ganti "Penelitian" dengan nama entity DB penelitian kamu
        {
            string title = "";
            string message = "";
            string type = "";

            if (status == "Diterima")
            {
                title   = "Selamat! Penelitian Diterima";
                message = $"Selamat {p.Nama}! Pengajuan penelitian kamu di {p.Company} ({p.Region}) telah DITERIMA.";
                type    = "success";
            }
            else if (status == "Ditolak")
            {
                title   = "Pengajuan Penelitian Ditolak";
                message = $"Mohon maaf {p.Nama}, pengajuan penelitian di {p.Company} ({p.Region}) tidak dapat kami terima saat ini.";
                type    = "error";
            }
            else
            {
                title   = "Update Status Penelitian";
                message = $"Status pengajuan penelitian kamu di {p.Company} diperbarui menjadi: {status}.";
                type    = "info";
            }

            var notif = new Notification
            {
                Nama       = p.Nama,
                Lokasi     = p.Region,
                Type       = type,
                UserEmail  = p.Email,
                Title      = title,
                Message    = message,
                Url        = "/DashboardPeserta#riwayat",
                CreatedAt  = DateTime.Now,
                IsRead     = false,
                ExternalId = p.Id.ToString()
            };

            _context.Notifications.Add(notif);
            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────
        // KIRIM EMAIL NOTIFIKASI MAGANG (tidak diubah)
        // ─────────────────────────────────────────────────────────────
        private async Task KirimEmailNotifikasi(Magang mhs, string status, string catatanRevisi = "")
        {
            var validStatuses = new[] { "Diterima", "Ditolak", "Revisi" };
            if (!validStatuses.Contains(status))
            {
                Console.WriteLine($"[EMAIL SKIP] Status '{status}' tidak memiliki template email.");
                return;
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a =>
                a.Region != null && mhs.Region != null &&
                a.Region.ToLower().Trim() == mhs.Region.ToLower().Trim());

            string root = _env.WebRootPath;

            string templateFileName = status == "Diterima" ? "EmailDiterima.html" :
                                      status == "Ditolak"  ? "EmailDitolak.html"  : "EmailRevisi.html";

            string rawFileName = status == "Diterima" ? "Diterima.txt" :
                                 status == "Ditolak"  ? "Ditolak.txt"  : "Revisi.txt";

            string pathHtml = Path.Combine(root, "templates", templateFileName);
            string pathTxt  = Path.Combine(root, "templates", "raw", rawFileName);

            if (!System.IO.File.Exists(pathHtml))
                throw new FileNotFoundException($"Template HTML tidak ditemukan: {pathHtml}");

            if (!System.IO.File.Exists(pathTxt))
                throw new FileNotFoundException($"Template TXT tidak ditemukan: {pathTxt}");

            var culture  = new CultureInfo("id-ID");
            string isiPesan = await System.IO.File.ReadAllTextAsync(pathTxt);

            isiPesan = isiPesan
                .Replace("{Nama}", mhs.NamaLengkap)
                .Replace("{Region}", mhs.Region)
                .Replace("{Unit}", mhs.Jurusan ?? "-")
                .Replace("{TanggalMulai}",  mhs.MulaiMagang.ToString("dd MMMM yyyy", culture))
                .Replace("{TanggalSelesai}", mhs.SelesaiMagang.ToString("dd MMMM yyyy", culture));

            if (status == "Revisi")
            {
                isiPesan = isiPesan.Replace("{KomentarRevisi}", catatanRevisi);
            }

            string htmlBody = await System.IO.File.ReadAllTextAsync(pathHtml);
            htmlBody = htmlBody
                .Replace("{IsiPesan}", isiPesan)
                .Replace("{Tahun}", DateTime.Now.Year.ToString());

            string subject = status == "Diterima" ? "Selamat! Seleksi Magang Diterima" :
                             status == "Revisi"   ? "Instruksi Revisi Data Pendaftaran" : "Informasi Seleksi Magang";

            await _emailService.SendWithCourierAsync(
                mhs.EmailPribadi,
                subject,
                htmlBody,
                "HC Pertamina - " + (admin?.Region ?? mhs.Region)
            );
        }

        // ─────────────────────────────────────────────────────────────
        // EXPORT MAHASISWA (tidak diubah)
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExportMahasiswa(string selectedRegion = "all")
        {
            var adminRole          = HttpContext.Session.GetString("AdminRole");
            var adminRegionManaged = HttpContext.Session.GetString("AdminRegion");

            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (!string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(adminRegionManaged))
                    query = query.Where(x => x.Region == adminRegionManaged);
            }
            else
            {
                if (selectedRegion != "all" && !string.IsNullOrEmpty(selectedRegion))
                    query = query.Where(x => x.Region == selectedRegion);
            }

            var data    = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data Mahasiswa");

                string[] headers = {
                    "ID", "Tgl Daftar", "NIM", "Nama Lengkap", "Email", "No HP",
                    "Instagram", "Tempat Lahir", "Tgl Lahir", "Universitas",
                    "Fakultas", "Jurusan", "Company", "Region", "Lokasi Unit",
                    "Rekomendasi", "Mulai", "Selesai", "Status",
                    "Link CV", "Link Surat", "Link Proposal", "Link Foto"
                };

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
                    worksheet.Cell(row, 1).Value  = item.Id;
                    worksheet.Cell(row, 2).Value  = item.CreatedAt.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 3).Value  = item.NIM;
                    worksheet.Cell(row, 4).Value  = item.NamaLengkap;
                    worksheet.Cell(row, 5).Value  = item.EmailPribadi;
                    worksheet.Cell(row, 6).Value  = item.NoHp;
                    worksheet.Cell(row, 7).Value  = item.Instagram ?? "-";
                    worksheet.Cell(row, 8).Value  = item.TempatLahir;
                    worksheet.Cell(row, 9).Value  = item.TanggalLahir.ToString("dd/MM/yyyy");
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

                    AddHyperlink(worksheet.Cell(row, 20), item.FileCv,             "cv",       baseUrl);
                    AddHyperlink(worksheet.Cell(row, 21), item.FileSuratPengantar, "surat",    baseUrl);
                    AddHyperlink(worksheet.Cell(row, 22), item.FileProposal,       "proposal", baseUrl);
                    AddHyperlink(worksheet.Cell(row, 23), item.FotoProfil,         "foto",     baseUrl);

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var regionLabel = (selectedRegion == "all" || string.IsNullOrEmpty(selectedRegion))
                                  ? "Semua_Region" : selectedRegion;

                if (!string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    regionLabel = adminRegionManaged ?? "Region";

                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Rekap_Magang_{regionLabel}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        private void AddHyperlink(IXLCell cell, string? fileName, string folder, string baseUrl)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                cell.Value = "-";
            }
            else
            {
                var cleanFileName = Path.GetFileName(fileName);
                var fullPath      = $"{baseUrl}/uploads/{folder}/{cleanFileName}";

                cell.Value = "Lihat Dokumen";
                cell.GetHyperlink().ExternalAddress = new Uri(fullPath);
                cell.Style.Font.FontColor = XLColor.Blue;
                cell.Style.Font.Underline = XLFontUnderlineValues.Single;
            }
        }
    }
}