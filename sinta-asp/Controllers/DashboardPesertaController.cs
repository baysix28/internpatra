using Microsoft.AspNetCore.Mvc;

namespace sinta_asp.Controllers
{
    public class DashboardPesertaController : Controller
    {
        public IActionResult Index()
        {
            // Di sini nanti kita bisa mengambil nama peserta dari Session/Database
            ViewBag.NamaPeserta = "Peserta SINTA"; 
            return View();
        }
    }
}