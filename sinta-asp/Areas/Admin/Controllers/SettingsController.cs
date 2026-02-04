using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; 
using sinta_asp.Data;
using AdminModel = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;

        public SettingsController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AdminModel>();
        }

        public async Task<IActionResult> Index()
        {
            var adminIdStr = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminIdStr))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            int adminId = int.Parse(adminIdStr);
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == adminId);

            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int UserId, string FullName, string Email)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(UserId);
                if (admin == null) return Json(new { success = false, message = "User tidak ditemukan" });

                admin.Nama = FullName;
                admin.Email = Email;

                _context.Update(admin);
                await _context.SaveChangesAsync();

                // UPDATE SESSION: Agar login tetap valid dengan identitas baru
                HttpContext.Session.SetString("AdminNama", FullName);
                HttpContext.Session.SetString("AdminEmail", Email);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string OldPassword, string NewPassword)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null) return Json(new { success = false, message = "User tidak ditemukan" });

                // Verifikasi Password Lama
                var result = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, OldPassword);
                if (result != PasswordVerificationResult.Success)
                {
                    return Json(new { success = false, message = "Kata sandi lama salah" });
                }

                if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Kata sandi baru minimal 6 karakter" });
                }

                // Hash dan Simpan Password Baru
                admin.PasswordHash = _passwordHasher.HashPassword(admin, NewPassword);
                _context.Update(admin);
                await _context.SaveChangesAsync();

                // Sesi tetap aman karena AdminId tidak berubah di database
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal: " + ex.Message });
            }
        }
    }
}