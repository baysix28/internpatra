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
        public IActionResult SaveTemplate(string type, string content)
        {
            try
            {
                // Menentukan apakah yang disimpan teks murni (raw) atau kode HTML
                // Jika tipenya mengandung 'Html', simpan ke file .html
                if (type.EndsWith("Html"))
                {
                    string realType = type.Replace("Html", "");
                    string fileName = realType == "Diterima" ? "EmailDiterima.html" : "EmailDitolak.html";
                    string path = Path.Combine(_env.WebRootPath, "templates", fileName);
                    System.IO.File.WriteAllText(path, content);
                }
                else
                {
                    // Simpan ke teks murni (raw) agar admin awam mudah mengedit
                    string fileName = type == "Diterima" ? "Diterima.txt" : "Ditolak.txt";
                    string path = Path.Combine(_env.WebRootPath, "templates", "raw", fileName);
                    System.IO.File.WriteAllText(path, content);
                }

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