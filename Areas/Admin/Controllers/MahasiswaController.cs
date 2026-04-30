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
            var adminNama = HttpContext.Session.GetString("AdminNama");
            var adminRole = HttpContext.Session.GetString("AdminRole"); 
            var adminRegionManaged = HttpContext.Session.GetString("AdminRegion");

            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            // MENGAMBIL REGION DINAMIS DARI DATABASE ADMIN
            var allRegionsInDb = await _context.Admins
                .Where(a => !string.IsNullOrEmpty(a.Region) && a.Region.ToLower() != "all")
                .Select(a => a.Region.Trim()) 
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            ViewBag.AllRegions = allRegionsInDb;
            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            // Logika Filter Berdasarkan Role
            if (!string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                // Jika bukan SuperAdmin, paksa filter ke Region milik admin tersebut
                if (!string.IsNullOrEmpty(adminRegionManaged))
                {
                    query = query.Where(x => x.Region == adminRegionManaged);
                    ViewBag.SelectedRegion = adminRegionManaged;
                }
            }
            else
            {
                // Jika SuperAdmin, bisa pilih semua atau region tertentu
                if (selectedRegion != "all" && !string.IsNullOrEmpty(selectedRegion))
                {
                    query = query.Where(x => x.Region == selectedRegion);
                    ViewBag.SelectedRegion = selectedRegion;
                }
                else
                {
                    ViewBag.SelectedRegion = "Semua Region";
                }
            }

            var magang = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();

            var viewModel = new DashboardModel
            {
                AdminName = adminNama,
                LoginTime = DateTime.Now,
                DaftarMagang = magang,
                StatusDiproses = magang.Count(x => x.Status == "Menunggu" || x.Status == "Proses Review"),
                StatusDiterima = magang.Count(x => x.Status == "Diterima"),
                StatusDitolak = magang.Count(x => x.Status == "Ditolak"),
                TotalInternAktif = magang.Count(x => x.Status == "Diterima")
            };

            ViewBag.AdminRole = adminRole;
            ViewBag.RawSelectedRegion = selectedRegion; 

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var rawRole = HttpContext.Session.GetString("AdminRole") ?? "";
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

                if (data.Status == status)
                    return Json(new { success = true, message = "Status sudah sesuai." });

                data.Status = status;
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

                return Json(new { success = true, message = "Status diperbarui dan email terkirim." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        private async Task KirimEmailNotifikasi(Magang mhs, string status)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => 
                a.Region != null && mhs.Region != null &&
                a.Region.ToLower().Trim() == mhs.Region.ToLower().Trim());

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

            await _emailService.SendWithCourierAsync(
                mhs.EmailPribadi, 
                status == "Diterima" ? "Selamat! Seleksi Magang Diterima" : "Informasi Seleksi Magang",
                htmlBody,
                "HC Pertamina - " + admin.Region
            );
        }

        [HttpGet]
        public async Task<IActionResult> ExportMahasiswa(string selectedRegion = "all")
        {
            var adminRole = HttpContext.Session.GetString("AdminRole");
            var adminRegionManaged = HttpContext.Session.GetString("AdminRegion");

            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (!string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
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

                var regionLabel = (selectedRegion == "all" || string.IsNullOrEmpty(selectedRegion)) 
                                  ? "Semua_Region" 
                                  : selectedRegion;

                if (!string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    regionLabel = adminRegionManaged ?? "Region";
                }

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Rekap_Magang_{regionLabel}_{DateTime.Now:yyyyMMdd}.xlsx");
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
                var fullPath = $"{baseUrl}/uploads/{folder}/{cleanFileName}";
                
                cell.Value = fullPath;
                cell.GetHyperlink().ExternalAddress = new Uri(fullPath);
                
                cell.Style.Font.FontColor = XLColor.Blue;
                cell.Style.Font.Underline = XLFontUnderlineValues.Single;
            }
        }
    }
}