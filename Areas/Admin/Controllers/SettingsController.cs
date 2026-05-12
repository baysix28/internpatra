using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Http;
using sinta_asp.Data;
using System;
using System.Threading.Tasks;
using AdminModel = sinta_asp.Models.Admin;
using Microsoft.AspNetCore.Authorization;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;

        public SettingsController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AdminModel>();
        }

        // --- Menampilkan Halaman Pengaturan ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var adminIdStr = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminIdStr))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            if (!int.TryParse(adminIdStr, out int adminId))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == adminId);
            if (admin == null) return NotFound();

            return View(admin);
        }

        // --- Update Profil (Nama & Email) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int UserId, string FullName, string Email)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(UserId);
                if (admin == null) 
                    return Json(new { success = false, message = "User tidak ditemukan" });

                // Update Data di Database
                admin.Nama = FullName;
                admin.Email = Email;

                _context.Update(admin);
                await _context.SaveChangesAsync();

                // SINKRONISASI SESSION: Agar nama di header dashboard langsung berubah
                HttpContext.Session.SetString("AdminNama", FullName);
                HttpContext.Session.SetString("AdminEmail", Email);

                return Json(new { success = true, message = "Profil berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        // --- Ganti Password ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string OldPassword, string NewPassword)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null) 
                    return Json(new { success = false, message = "User tidak ditemukan" });

                // 1. Verifikasi apakah password lama benar
                var verificationResult = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, OldPassword);
                if (verificationResult != PasswordVerificationResult.Success)
                {
                    return Json(new { success = false, message = "Kata sandi lama yang Anda masukkan salah" });
                }

                // 2. Validasi panjang password baru
                if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Kata sandi baru minimal harus 6 karakter" });
                }

                // 3. Hash password baru dan simpan
                admin.PasswordHash = _passwordHasher.HashPassword(admin, NewPassword);
                
                _context.Update(admin);
                await _context.SaveChangesAsync();

                // Catatan: User tidak perlu logout karena AdminId di session tetap valid
                return Json(new { success = true, message = "Kata sandi berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal memperbarui kata sandi: " + ex.Message });
            }
        }
    }
}