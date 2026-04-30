using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using sinta_asp.Models;

namespace sinta_asp.Controllers
{
    public class HomeController : Controller
    {
        // Homepage publik
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}