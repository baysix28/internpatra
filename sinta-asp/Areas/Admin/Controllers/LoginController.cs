using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; 
using sinta_asp.Data;
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

            var existingAdmin = await _context.Admins.AnyAsync(a => a.Email == Email);
            if (existingAdmin)
            {
                return Json(new { success = false, message = "Email sudah digunakan oleh akun lain" });
            }

            var newAdmin = new AdminModel
            {
                Nama = FullName,
                Email = Email
            };

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

        // ==========================================
        // POST: /Admin/Login/ForgotPassword
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return Json(new { success = false, message = "Email wajib diisi" });
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);
            if (admin == null)
            {
                return Json(new { success = false, message = "Email tidak terdaftar di sistem kami." });
            }

            string temporaryPassword = Guid.NewGuid().ToString().Substring(0, 8);
            admin.PasswordHash = _passwordHasher.HashPassword(admin, temporaryPassword);

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                var smtpHost = _config["EmailSettings:Host"];
                var smtpPort = int.Parse(_config["EmailSettings:Port"]);
                var senderEmail = _config["EmailSettings:Email"];
                var senderPass = _config["EmailSettings:Password"];
                
                using (var smtpClient = new SmtpClient(smtpHost))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPass);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "SINTA Pertamina Support"),
                        Subject = "Reset Password Admin SINTA",
                        Body = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee;'>
                                <h2 style='color: #00549B;'>Halo, {admin.Nama}</h2>
                                <p>Kami telah mereset password akun SINTA Anda.</p>
                                <p>Gunakan password sementara berikut untuk login:</p>
                                <div style='background: #f4f4f4; padding: 15px; font-size: 24px; font-weight: bold; color: #E30613; text-align: center;'>
                                    {temporaryPassword}
                                </div>
                                <p>Mohon segera ganti password Anda setelah login demi keamanan.</p>
                            </div>",
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