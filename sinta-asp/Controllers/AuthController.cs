using Microsoft.AspNetCore.Mvc;
using sinta_asp.Data; // Sesuaikan dengan namespace AppDbContext Anda
using sinta_asp.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace sinta_asp.Controllers
{
    public class AuthController : Controller
    {
        // 1. Definisikan variabel context
        private readonly AppDbContext _context;

        // 2. Masukkan context ke dalam Constructor
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            var account = _context.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (account != null)
            {
                // Membuat identitas user (KTP Digital)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Email),
                    new Claim("Id", account.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Proses "Masuk" ke sistem
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "DashboardPeserta");
            }
            
            TempData["ErrorMessage"] = "Akun tidak ditemukan. Silakan daftar dulu!";
            return RedirectToAction("Login");
        }
        
        // --- TAMBAHKAN INI ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Ini untuk memproses data saat tombol "Daftar" di klik
        [HttpPost]
        public IActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Registrasi Berhasil! Silakan Login."; // Tambahkan ini agar alert muncul
                return RedirectToAction("Login");
            }
            return View(model);
        }
    }
}