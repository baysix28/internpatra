using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using sinta_asp.Data;
using System.Threading.Tasks;

// ALIAS MODEL ADMIN
using AdminModel = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;

        public LoginController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AdminModel>();
        }

        // ===============================
        // GET: /Admin/Login
        // ===============================
        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("AdminLogin") == "true")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            return View();
        }

        // ===============================
        // POST: /Admin/Login/Index (LOGIN)
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Index(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Email dan password wajib diisi" });
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);

            if (admin == null)
            {
                return Json(new { success = false, message = "Email tidak ditemukan" });
            }

            var verify = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, Password);

            if (verify != PasswordVerificationResult.Success)
            {
                return Json(new { success = false, message = "Password salah" });
            }

            // SET SESSION
            HttpContext.Session.SetString("AdminLogin", "true");
            HttpContext.Session.SetString("AdminId", admin.Id.ToString());
            HttpContext.Session.SetString("AdminNama", admin.Nama ?? "Admin");
            HttpContext.Session.SetString("AdminEmail", admin.Email);

            return Json(new { success = true });
        }

        // ===============================
        // POST: /Admin/Login/Register (PENDAFTARAN)
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Register(string FullName, string Email, string Password)
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Semua field wajib diisi" });
            }

            // 1. Cek apakah email sudah terdaftar
            var existingAdmin = await _context.Admins.AnyAsync(a => a.Email == Email);
            if (existingAdmin)
            {
                return Json(new { success = false, message = "Email sudah digunakan oleh akun lain" });
            }

            // 2. Buat objek admin baru
            var newAdmin = new AdminModel
            {
                Nama = FullName,
                Email = Email
            };

            // 3. Hash password sebelum disimpan
            newAdmin.PasswordHash = _passwordHasher.HashPassword(newAdmin, Password);

            try
            {
                _context.Admins.Add(newAdmin);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Registrasi berhasil! Silakan login." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan saat menyimpan data: " + ex.Message });
            }
        }

        // ===============================
        // LOGOUT
        // ===============================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}