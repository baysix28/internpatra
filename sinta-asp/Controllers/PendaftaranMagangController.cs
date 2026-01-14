using Microsoft.AspNetCore.Mvc;
using sinta_asp.Models;

namespace sinta_asp.Controllers
{
    public class PendaftaranMagangController : Controller
    {
        // Menampilkan Form: localhost:xxxx/PendaftaranMagang
        public IActionResult Index()
        {
            return View();
        }

        // Proses Simpan: POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Store(PendaftaranRequest request)
        {
            if (ModelState.IsValid)
            {
                // Logika simpan ke database diletakkan di sini
                // Untuk sementara kita redirect ke halaman sukses
                return RedirectToAction("Hasil");
            }
            
            // Jika validasi gagal, balik ke form
            return View("Index", request);
        }

        public IActionResult Hasil()
        {
            return View();
        }

        public IActionResult DataDiri()
        {
            return View();
        }

        public IActionResult DataKampus()
        {
            return View();
        }
    }
}