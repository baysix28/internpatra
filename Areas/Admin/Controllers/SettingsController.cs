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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string Nama, string Email)
        {
            try
            {
                var adminIdStr = HttpContext.Session.GetString("AdminId");
                if (string.IsNullOrEmpty(adminIdStr) || !int.TryParse(adminIdStr, out int adminId))
                    return Json(new { success = false, message = "Sesi tidak valid, silakan login ulang." });

                var admin = await _context.Admins.FindAsync(adminId);
                if (admin == null) 
                    return Json(new { success = false, message = "User tidak ditemukan." });

                if (string.IsNullOrWhiteSpace(Nama))
                    return Json(new { success = false, message = "Nama tidak boleh kosong." });

                if (string.IsNullOrWhiteSpace(Email))
                    return Json(new { success = false, message = "Email tidak boleh kosong." });

                // Update Data di Database
                admin.Nama = Nama;
                admin.Email = Email;

                _context.Update(admin);
                await _context.SaveChangesAsync();

                // SINKRONISASI SESSION: Agar nama di header dashboard langsung berubah
                HttpContext.Session.SetString("AdminNama", Nama);
                HttpContext.Session.SetString("AdminEmail", Email);

                return Json(new { success = true, message = "Profil berhasil diperbarui.", nama = Nama, email = Email });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            try
            {
                var adminIdStr = HttpContext.Session.GetString("AdminId");
                if (string.IsNullOrEmpty(adminIdStr) || !int.TryParse(adminIdStr, out int adminId))
                    return Json(new { success = false, message = "Sesi tidak valid, silakan login ulang." });

                var admin = await _context.Admins.FindAsync(adminId);
                if (admin == null)
                    return Json(new { success = false, message = "Admin tidak ditemukan." });

                if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
                    return Json(new { success = false, message = "Kata sandi baru minimal harus 6 karakter." });

                if (NewPassword != ConfirmPassword)
                    return Json(new { success = false, message = "Konfirmasi kata sandi tidak cocok." });

                // Cek format password di database: sudah hash atau masih plain text
                bool isOldPasswordValid = false;

                if (IsValidIdentityHash(admin.PasswordHash))
                {
                    // Sudah di-hash dengan PasswordHasher -> verifikasi normal
                    var result = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, OldPassword);
                    isOldPasswordValid = result == PasswordVerificationResult.Success
                                     || result == PasswordVerificationResult.SuccessRehashNeeded;
                }
                else
                {
                    // Masih plain text di database -> bandingkan langsung
                    isOldPasswordValid = admin.PasswordHash == OldPassword;
                }

                if (!isOldPasswordValid)
                    return Json(new { success = false, message = "Kata sandi lama yang Anda masukkan salah." });

                // Simpan sebagai hash yang benar (mulai sekarang sudah aman)
                admin.PasswordHash = _passwordHasher.HashPassword(admin, NewPassword);
                _context.Update(admin);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Kata sandi berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal memperbarui kata sandi: " + ex.Message });
            }
        }
        private static bool IsValidIdentityHash(string? hash)
        {
            if (string.IsNullOrWhiteSpace(hash)) return false;
            try
            {
                var bytes = Convert.FromBase64String(hash);
                // ASP.NET Identity v2 hash: 49 bytes, v3 hash: 61 bytes
                return bytes.Length == 49 || bytes.Length == 61;
            }
            catch
            {
                return false;
            }
        }
    }
}