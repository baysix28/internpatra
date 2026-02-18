using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using sinta_asp.Data;
using sinta_asp.Models;
using sinta_asp.Services; 
using System;
using System.Threading.Tasks;
using AdminModel = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;

        public AccountController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AdminModel>();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AdminId") != null)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            return View("~/Areas/Admin/Views/Login/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Email dan Password wajib diisi." });
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);

            if (admin == null)
            {
                return Json(new { success = false, message = "Email tidak terdaftar." });
            }

            // VERIFIKASI PASSWORD (Mendukung Hashing dari SettingsController)
            // Jika Anda sebelumnya pakai admin.Password biasa, ubah kolomnya ke PasswordHash di database
            var result = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, Password);
            
            if (result == PasswordVerificationResult.Failed)
            {
                return Json(new { success = false, message = "Password salah." });
            }

            // SET SESSION (Data terbaru langsung masuk sini)
            HttpContext.Session.SetString("AdminId", admin.Id.ToString());
            HttpContext.Session.SetString("AdminNama", admin.Nama);
            HttpContext.Session.SetString("AdminEmail", admin.Email);
            HttpContext.Session.SetString("AdminRole", admin.Role ?? "Admin");
            HttpContext.Session.SetString("AdminRegion", admin.Region ?? ""); 

            return Json(new { success = true, message = "Login Berhasil! Mengalihkan..." });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email, [FromServices] IEmailService emailService)
        {
            if (string.IsNullOrEmpty(Email)) return Json(new { success = false, message = "Email wajib diisi." });

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);
            if (admin != null)
            {
                admin.ResetToken = Guid.NewGuid().ToString();
                admin.ResetTokenExpiry = DateTime.Now.AddHours(1);
                await _context.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account", 
                    new { area = "Admin", token = admin.ResetToken }, Request.Scheme);

                await emailService.SendForgotPasswordEmailAsync(Email, resetLink);
            }

            return Json(new { success = true, message = "Jika email terdaftar, instruksi reset telah dikirim." });
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account", new { area = "Admin" });
            ViewBag.Token = token;
            return View("~/Areas/Admin/Views/Login/ResetPassword.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string Token, string NewPassword)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => 
                a.ResetToken == Token && a.ResetTokenExpiry > DateTime.Now);

            if (admin == null)
                return Json(new { success = false, message = "Token tidak valid atau kedaluwarsa." });

            // Simpan password baru dalam bentuk HASH (Agar sinkron dengan Settings)
            admin.PasswordHash = _passwordHasher.HashPassword(admin, NewPassword);
            admin.ResetToken = null;
            admin.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Password berhasil diperbarui! Silakan login." });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }
    }
}