<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Mvc;
=======
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics; // Tambahan biar 'Activity' ga error
using sinta_asp.Models;   // Tambahan biar 'ErrorViewModel' ga error
>>>>>>> 659a81f9878d152c3c8220b7520b93e73f755cfb

namespace sinta_asp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
<<<<<<< HEAD
            // Redirect langsung ke Admin Login
            return RedirectToAction("Index", "Login", new { area = "Admin" });

            // ATAU tampilkan halaman welcome:
            // return View();
        }

=======
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
>>>>>>> 659a81f9878d152c3c8220b7520b93e73f755cfb
        public IActionResult Error()
        {
            return View();
        }
    }
}