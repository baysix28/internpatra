using Microsoft.AspNetCore.Mvc;
using sinta_asp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

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

        // =======================
        // LOGIN PAGE
        // =======================
        public IActionResult Index()
        {
            return View();
        }

        // =======================
        // PROSES LOGIN
        // =======================
        [HttpPost]
        public async Task<IActionResult> Index(string Email, string Password)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == Email);

            if (admin == null)
                return Json(new { success = false, message = "Email tidak ditemukan" });

            if (!BCrypt.Net.BCrypt.Verify(Password, admin.PasswordHash))
                return Json(new { success = false, message = "Password salah" });

            HttpContext.Session.SetString("AdminLogin", "true");
            HttpContext.Session.SetString("AdminEmail", admin.Email);

            return Json(new { success = true });
        }
    }
}
