using Microsoft.AspNetCore.Mvc;
using sinta_asp.Models;

namespace sinta_asp.Controllers
{
    public class PendaftaranMagangController : Controller
    {
        // Menampilkan halaman info program magang
        // GET: /PendaftaranMagang
        public IActionResult Index()
        {
            return View();
        }

        // Menampilkan form pendaftaran
        // GET: /PendaftaranMagang/DataDiri
        public IActionResult DataDiri()
        {
            return View();
        }

        // Proses Simpan Data
        // POST: /PendaftaranMagang/Store
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Store(PendaftaranRequest request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // TODO: Logika simpan ke database
                    // - Simpan data peserta
                    // - Upload file (CV, Surat Pengantar, Proposal)
                    // - Generate nomor pendaftaran
                    // - Kirim email notifikasi

                    // Sementara redirect ke halaman sukses
                    return RedirectToAction("Sukses");
                }
                catch (Exception ex)
                {
                    // Handle error
                    TempData["Error"] = "Terjadi kesalahan: " + ex.Message;
                    return View("DataDiri", request);
                }
            }
            
            // Jika validasi gagal, balik ke form
            return View("DataDiri", request);
        }

        // Halaman sukses setelah pendaftaran
        // GET: /PendaftaranMagang/Sukses
        public IActionResult Sukses()
        {
            return View();
        }

        // Halaman hasil/status pendaftaran
        // GET: /PendaftaranMagang/Hasil
        public IActionResult Hasil()
        {
            return View();
        }

        // Halaman data magang (jika diperlukan)
        // GET: /PendaftaranMagang/DataMagang
        public IActionResult DataMagang()
        {
            return View();
        }
    }
}