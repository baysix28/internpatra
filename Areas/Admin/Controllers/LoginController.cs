using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; 
using sinta_asp.Data;
using sinta_asp.Models;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System;

// ALIAS MODEL ADMIN
using AdminModel = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;
        private readonly IConfiguration _config;

        public LoginController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

            // --- PROTEKSI AKTIVASI (UPDATE: BYPASS UNTUK SUPERADMIN) ---
            // Jika akun bukan SuperAdmin DAN belum aktif, maka blokir login.
            if (admin.Role != "SuperAdmin" && !admin.IsActive)
            {
                return Json(new { success = false, message = "Akun Anda belum aktif! Silakan cek email untuk aktivasi." });
            }

            var verify = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, Password);

            if (verify != PasswordVerificationResult.Success)
            {
                return Json(new { success = false, message = "Password salah" });
            }

            // ======================================================
            // SET SESSION
            // ======================================================
            HttpContext.Session.SetString("AdminLogin", "true");
            HttpContext.Session.SetString("AdminId", admin.Id.ToString());
            HttpContext.Session.SetString("AdminNama", admin.Nama ?? "Admin");
            HttpContext.Session.SetString("AdminEmail", admin.Email);
            HttpContext.Session.SetString("AdminRole", (admin.Role ?? "Admin").Trim());
            HttpContext.Session.SetString("AdminRegion", admin.Region ?? "");

            return Json(new { success = true });
        }

        // ===============================
        // POST: /Admin/Login/Register (PENDAFTARAN)
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Register(string FullName, string Email, string Password, string Role = "Admin")
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Semua field wajib diisi" });
            }

            var existingAdmin = await _context.Admins.AnyAsync(a => a.Email == Email);
            if (existingAdmin)
            {
                return Json(new { success = false, message = "Email sudah digunakan oleh akun lain" });
            }

            // --- INISIALISASI AKUN (UPDATE: SUPERADMIN OTOMATIS AKTIF) ---
            var newAdmin = new AdminModel
            {
                Nama = FullName,
                Email = Email,
                Role = Role,
                Region = "Pusat",
                // Jika yang didaftarkan adalah SuperAdmin, maka langsung aktif tanpa token
                IsActive = (Role == "SuperAdmin"), 
                ActivationToken = (Role == "SuperAdmin") ? null : Guid.NewGuid().ToString()
            };

            newAdmin.PasswordHash = _passwordHasher.HashPassword(newAdmin, Password);

            try
            {
                _context.Admins.Add(newAdmin);
                await _context.SaveChangesAsync();
                
                string successMessage = (Role == "SuperAdmin") 
                    ? "Registrasi SuperAdmin berhasil! Silakan langsung login." 
                    : "Registrasi berhasil! Silakan cek email untuk aktivasi.";

                return Json(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        // ==========================================
        // PROSES AKTIVASI DARI LINK EMAIL
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ActivateAccount(string token)
        {
            if (string.IsNullOrEmpty(token)) return Content("Token tidak valid.");

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.ActivationToken == token);

            if (admin == null)
            {
                return Content("Link aktivasi tidak valid atau sudah digunakan.");
            }

            admin.IsActive = true;
            admin.ActivationToken = null; // Hapus token
            
            _context.Update(admin);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { message = "Akun berhasil diaktifkan. Silakan login." });
        }

        // ==========================================
        // POST: /Admin/Login/ForgotPassword
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email)) return Json(new { success = false, message = "Email wajib diisi" });

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);
            if (admin == null)
            {
                return Json(new { success = false, message = "Email tidak terdaftar." });
            }

            // --- PASSWORD SEMENTARA ---
            string temporaryPassword = Guid.NewGuid().ToString().Substring(0, 8);
            admin.PasswordHash = _passwordHasher.HashPassword(admin, temporaryPassword);

            try
            {
                var smtpHost = _config["EmailSettings:Host"];
                var smtpPort = int.Parse(_config["EmailSettings:Port"] ?? "587");
                var senderEmail = _config["EmailSettings:Email"];
                var senderPass = _config["EmailSettings:Password"];

                using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPass);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "SINTA Pertamina Support"),
                        Subject = "Reset Password Admin SINTA",
                        Body = $"<p>Halo {admin.Nama}, password sementara Anda adalah: <b>{temporaryPassword}</b></p>",
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(Email);

                    await smtpClient.SendMailAsync(mailMessage);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Password sementara telah dikirim ke email." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal kirim email: " + ex.Message });
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