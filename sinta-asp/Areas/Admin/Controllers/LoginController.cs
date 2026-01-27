using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using sinta_asp.Data;
using sinta_asp.Models;
using System.Linq;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ✅ INI TEMPAT YANG BENAR
        [HttpPost]
        public IActionResult Index(string Email, string Password)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Email == Email);

            if (admin != null && BCrypt.Net.BCrypt.Verify(Password, admin.PasswordHash))
            {
                // SIMPAN SESSION LOGIN
                HttpContext.Session.SetString("AdminLogin", "true");
                HttpContext.Session.SetString("AdminEmail", admin.Email);
                HttpContext.Session.SetString("AdminName", admin.Nama);

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Email atau Password salah!" });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
