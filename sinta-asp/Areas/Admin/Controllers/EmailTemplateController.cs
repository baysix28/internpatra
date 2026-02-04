using Microsoft.AspNetCore.Mvc;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
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

            // Baca konten teks murni (.txt) untuk ditampilkan di editor
            ViewBag.DiterimaRaw = GetRawContent("Diterima.txt");
            ViewBag.DitolakRaw = GetRawContent("Ditolak.txt");

            // Baca konten HTML (.html) hanya untuk fitur "Lihat Kode" (Opsional)
            ViewBag.DiterimaHtml = GetHtmlContent("EmailDiterima.html");
            ViewBag.DitolakHtml = GetHtmlContent("EmailDitolak.html");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveTemplate(string type, string content)
        {
            try
            {
                string fileName;
                string subFolder = "";

                if (type.EndsWith("Html"))
                {
                    string realType = type.Replace("Html", "");
                    fileName = realType == "Diterima" ? "EmailDiterima.html" : "EmailDitolak.html";
                }
                else
                {
                    subFolder = "raw";
                    fileName = type == "Diterima" ? "Diterima.txt" : "Ditolak.txt";
                }

                string path = Path.Combine(_env.WebRootPath, "templates", subFolder, fileName);
                
                // Pastikan direktori ada
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                await System.IO.File.WriteAllTextAsync(path, content, System.Text.Encoding.UTF8);
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
            return "Yth. Sdr/i {Nama},\n\n(Teks belum diatur)";
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