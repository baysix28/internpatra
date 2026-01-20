using Microsoft.AspNetCore.Mvc;

namespace sinta_asp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect langsung ke Admin Login
            return RedirectToAction("Index", "Login", new { area = "Admin" });

            // ATAU tampilkan halaman welcome:
            // return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}