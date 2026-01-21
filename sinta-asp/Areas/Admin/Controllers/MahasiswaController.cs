using Microsoft.AspNetCore.Mvc;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MahasiswaController : Controller
    {
        public IActionResult Index()
        {
            // Tidak memanggil database, hanya mengembalikan tampilan
            return View();
        }
    }
}