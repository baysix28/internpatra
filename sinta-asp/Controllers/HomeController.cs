using Microsoft.AspNetCore.Mvc;

namespace sinta_asp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Menampilkan Views/Home/Index.cshtml (Dashboard)
            return View();
        }
    }
}