using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using sinta_asp.Models;

namespace sinta_asp.Controllers
{
    public class HomeController : Controller
    {
        // HOME USER
        public IActionResult Index()
        {
            // Menampilkan Views/Home/Index.cshtml
            return View();
        }

        public IActionResult Dashboard()
        {
            return View(); // Views/Home/Dashboard.cshtml
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
