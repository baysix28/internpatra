using Microsoft.AspNetCore.Mvc; 

namespace sinta_asp.Controllers
{
    // Nama class harus sama dengan nama file
    public class HasilPendaftaranController : Controller 
    {
        // URL akses: localhost:5109/HasilPendaftaran
        public IActionResult Index()
        {
            return View();
        }
    }
}