using Microsoft.AspNetCore.Mvc;
using System.Diagnostics; // Tambahan biar 'Activity' ga error
using sinta_asp.Models;   // Tambahan biar 'ErrorViewModel' ga error

namespace sinta_asp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Menampilkan Views/Home/Index.cshtml (Dashboard)
            return View();
        }

        // --- INI KITA PERTAHANKAN DARI MASTER ---
        public IActionResult Dashboard()
        {
            return View(); // Ini nanti nyari file Views/Home/Dashboard.cshtml
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}