using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]
    public class EmailTemplateController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            // Pastikan folder raw sudah ada
            string rawPath = Path.Combine(_env.WebRootPath, "templates", "raw");
            if (!Directory.Exists(rawPath)) Directory.CreateDirectory(rawPath);
            string contentRevisi = GetRawContent("Revisi.txt");
            // Baca konten teks murni (.txt) untuk editor
            ViewBag.DiterimaRaw = GetRawContent("Diterima.txt");
            ViewBag.DitolakRaw = GetRawContent("Ditolak.txt");
            ViewBag.RevisiRaw = contentRevisi; // Tambahan Revisi

            // Baca konten HTML (.html)
            ViewBag.DiterimaHtml = GetHtmlContent("EmailDiterima.html");
            ViewBag.DitolakHtml = GetHtmlContent("EmailDitolak.html");
            ViewBag.RevisiHtml = GetHtmlContent("EmailRevisi.html"); // Tambahan Revisi

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveTemplate(string type, string? content)
        {
            try
            {
                string fileName;
                string subFolder = "";
                string safeContent = content ?? ""; // Hindari null content

                if (type.EndsWith("Html"))
                {
                    string realType = type.Replace("Html", "");
                    fileName = realType switch
                    {
                        "Diterima" => "EmailDiterima.html",
                        "Ditolak" => "EmailDitolak.html",
                        "Revisi" => "EmailRevisi.html",
                        _ => throw new Exception("Tipe template tidak dikenal")
                    };
                }
                else
                {
                    subFolder = "raw";
                    fileName = type switch
                    {
                        "Diterima" => "Diterima.txt",
                        "Ditolak" => "Ditolak.txt",
                        "Revisi" => "Revisi.txt",
                        _ => throw new Exception("Tipe template tidak dikenal")
                    };
                }

                string path = Path.Combine(_env.WebRootPath, "templates", subFolder, fileName);
                string? directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(directory)) 
                {
                    if (!Directory.Exists(directory)) 
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                // --- JANGAN LUPA BARIS INI SOPHIE ---
                await System.IO.File.WriteAllTextAsync(path, safeContent, System.Text.Encoding.UTF8);
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private string GetRawContent(string fileName)
        {
            string path = Path.Combine(_env.WebRootPath, "templates", "raw", fileName);
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllText(path);
            }
            return "Yth. Sdr/i {Nama},\n\n(Teks format untuk " + fileName.Replace(".txt", "") + " belum diatur)";
        }

        private string GetHtmlContent(string fileName)
        {
            string path = Path.Combine(_env.WebRootPath, "templates", fileName);
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllText(path);
            }
            return "";
        }
    }
}