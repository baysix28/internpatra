using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using sinta_asp.Data;
using sinta_asp.Models;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;

// ALIAS MODEL ADMIN (Pastikan namespace model Admin Anda benar)
using AdminModel = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]

    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;
        private readonly IConfiguration _config;

        public AccountController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _passwordHasher = new PasswordHasher<AdminModel>();
        }

        // ==========================================
        // TAMPILAN HALAMAN LOGIN
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            // Jika sesi aktif ditemukan, langsung arahkan ke Dashboard
            if (HttpContext.Session.GetString("AdminLogin") == "true")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return View("~/Areas/Admin/Views/Login/Index.cshtml");
        }

        // ==========================================
        // PROSES LOGIN
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Email dan Password wajib diisi." });
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);

            // 1. Validasi keberadaan user
            if (admin == null)
            {
                return Json(new { success = false, message = "Email tidak terdaftar." });
            }

            // ======================================================
            // 2. PROTEKSI AKTIVASI (UPDATE: BYPASS UNTUK SUPERADMIN)
            // ======================================================
            // Jika user BUKAN SuperAdmin, maka wajib dicek status IsActive-nya.
            // Jika user ADALAH SuperAdmin, dia bisa login walau IsActive = false.
            if (admin.Role != "SuperAdmin" && !admin.IsActive)
            {
                return Json(new { 
                    success = false, 
                    message = "Akun Anda belum aktif! Silakan cek kotak masuk atau spam email Anda untuk aktivasi." 
                });
            }

            // 3. VERIFIKASI PASSWORD
            var result = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Json(new { success = false, message = "Password yang Anda masukkan salah." });
            }

            // 4. SET SESSION AUTENTIKASI
            HttpContext.Session.SetString("AdminLogin", "true");
            HttpContext.Session.SetString("AdminId", admin.Id.ToString());
            HttpContext.Session.SetString("AdminNama", admin.Nama);
            HttpContext.Session.SetString("AdminEmail", admin.Email);
            HttpContext.Session.SetString("AdminRole", (admin.Role ?? "Admin").Trim());
            HttpContext.Session.SetString("AdminRegion", admin.Region ?? "Nasional");

            return Json(new { success = true, message = "Login Berhasil! Mengalihkan..." });
        }

        // ==========================================
        // PROSES AKTIVASI DARI LINK EMAIL
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ActivateAccount(string token)
        {
            if (string.IsNullOrEmpty(token)) return Content("Token aktivasi tidak valid.");

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.ActivationToken == token);

            if (admin == null)
            {
                return Content("Link aktivasi tidak valid, kadaluwarsa, atau sudah diaktifkan.");
            }

            // Update status akun menjadi aktif
            admin.IsActive = true;
            admin.ActivationToken = null; // Token dihapus agar tidak bisa dipakai lagi
            
            _context.Update(admin);
            await _context.SaveChangesAsync();

            // Redirect ke halaman login dengan pesan sukses
            return RedirectToAction("Login", new { activated = true });
        }

        // ==========================================
        // FORGOT PASSWORD (Kirim Link Reset)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email)) return Json(new { success = false, message = "Email wajib diisi." });

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);
            
            // Link reset hanya dikirim jika akun terdaftar (SuperAdmin dibolehkan reset walau belum aktivasi)
            if (admin != null && (admin.IsActive || admin.Role == "SuperAdmin"))
            {
                admin.ResetToken = Guid.NewGuid().ToString();
                admin.ResetTokenExpiry = DateTime.Now.AddHours(2);
                await _context.SaveChangesAsync();

                var resetLink = $"{Request.Scheme}://{Request.Host}/Admin/Account/ResetPassword?token={admin.ResetToken}";
                
                string body = $@"
                    <h3>Reset Password SINTA</h3>
                    <p>Halo {admin.Nama}, Anda meminta untuk mereset password.</p>
                    <p>Silakan klik link di bawah ini (berlaku 2 jam):</p>
                    <a href='{resetLink}'>Reset Password Saya</a>";

                await SendEmailAsync(admin.Email, "Reset Password Administrator SINTA", body);
            }

            // Pesan dibuat umum demi keamanan
            return Json(new { success = true, message = "Jika email terdaftar, instruksi reset telah dikirim ke email Anda." });
        }

        // ==========================================
        // RESET PASSWORD (Form Input Password Baru)
        // ==========================================
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");
            ViewBag.Token = token;
            return View("~/Areas/Admin/Views/Login/ResetPassword.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string Token, string NewPassword)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => 
                a.ResetToken == Token && a.ResetTokenExpiry > DateTime.Now);

            if (admin == null)
                return Json(new { success = false, message = "Token reset tidak valid atau sudah kadaluwarsa." });

            admin.PasswordHash = _passwordHasher.HashPassword(admin, NewPassword);
            admin.ResetToken = null;
            admin.ResetTokenExpiry = null;
            admin.IsActive = true;
            
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Password berhasil diperbarui! Silakan login." });
        }

        // ==========================================
        // LOGOUT
        // ==========================================
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ==========================================
        // PRIVATE HELPER: KIRIM EMAIL SMTP
        // ==========================================
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var senderEmail = _config["EmailSettings:Email"] ?? "";
            var senderPass = _config["EmailSettings:Password"];

            using (var client = new SmtpClient(host, port))
            {
                client.Credentials = new NetworkCredential(senderEmail, senderPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "SINTA Support"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}