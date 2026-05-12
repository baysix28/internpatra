using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using sinta_asp.Services;


using AdminEntity = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminEntity> _passwordHasher;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        

        public LoginController(AppDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config  = config;
            _emailService = emailService;
            _passwordHasher = new PasswordHasher<AdminEntity>();
        }

        // ================= GET LOGIN =================
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (!string.IsNullOrEmpty(returnUrl) &&
                returnUrl.Contains("/Admin/Login", StringComparison.OrdinalIgnoreCase))
                returnUrl = null;

            ViewBag.ReturnUrl = returnUrl;
            return View("~/Areas/Admin/Views/Login/Index.cshtml");
        }

        // ================= POST LOGIN =================
        [HttpPost]
        public async Task<IActionResult> Index(string Email, string Password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return Json(new { success = false, message = "Email dan password wajib diisi" });

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Email);

            if (admin == null)
                return Json(new { success = false, message = "Email tidak ditemukan" });

            // FIX: Gunakan "SuperAdmin" konsisten (sesuai Role di model)
            if (admin.Role != "SuperAdmin" && !admin.IsActive)
                return Json(new { success = false, message = "Akun belum aktif. Cek email Anda untuk link aktivasi." });

            // 🔥 CEK OTP (kode reset)
            if (admin.ResetToken == Password && admin.ResetTokenExpiry > DateTime.UtcNow)
            {
                admin.ResetToken = null;
                admin.ResetTokenExpiry = null;
                await _context.SaveChangesAsync();

                var claimsOtp = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,  admin.Email ?? ""),
                    new Claim(ClaimTypes.Role,  admin.Role  ?? "AdminRegion"),
                    new Claim("AdminId",   admin.Id.ToString()),
                    new Claim("AdminNama", admin.Nama ?? "")
                };

                var identityOtp = new ClaimsIdentity(claimsOtp, "AdminScheme");

                await HttpContext.SignInAsync(
                    "AdminScheme",
                    new ClaimsPrincipal(identityOtp),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(12)
                    });

                // FIX: Semua key session konsisten, pakai "AdminNama" bukan "AdminName"
                HttpContext.Session.SetString("AdminLogin",  "true");
                HttpContext.Session.SetString("AdminId",     admin.Id.ToString());
                HttpContext.Session.SetString("AdminNama",   admin.Nama   ?? "");
                HttpContext.Session.SetString("AdminEmail",  admin.Email  ?? "");
                HttpContext.Session.SetString("AdminRole",   admin.Role   ?? "AdminRegion");
                HttpContext.Session.SetString("AdminRegion", admin.Region ?? "");

                return Json(new { success = true, redirect = "/Admin/Dashboard" });
            }

            // 🔐 LOGIN NORMAL (password)
            var verify = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, Password);

            if (verify != PasswordVerificationResult.Success)
                return Json(new { success = false, message = "Password salah" });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  admin.Email ?? ""),
                new Claim(ClaimTypes.Role,  admin.Role  ?? "AdminRegion"),
                new Claim("AdminId",   admin.Id.ToString()),
                new Claim("AdminNama", admin.Nama ?? "")
            };

            var identity = new ClaimsIdentity(claims, "AdminScheme");

            await HttpContext.SignInAsync(
                "AdminScheme",
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(12)
                });

            // FIX: Semua key session konsisten, pakai "AdminNama" bukan "AdminName"
            HttpContext.Session.SetString("AdminLogin",  "true");
            HttpContext.Session.SetString("AdminId",     admin.Id.ToString());
            HttpContext.Session.SetString("AdminNama",   admin.Nama   ?? "");
            HttpContext.Session.SetString("AdminEmail",  admin.Email  ?? "");
            HttpContext.Session.SetString("AdminRole",   admin.Role   ?? "AdminRegion");
            HttpContext.Session.SetString("AdminRegion", admin.Region ?? "");

            string redirectUrl = "/Admin/Dashboard";
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl) &&
                !returnUrl.Contains("/Admin/Login", StringComparison.OrdinalIgnoreCase))
                redirectUrl = returnUrl;

            return Json(new { success = true, redirect = redirectUrl });
        }

        // ================= POST FORGOT PASSWORD =================
        [HttpPost]
        [Route("Admin/Login/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Email wajib diisi." });

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == email);

            if (admin == null)
                return Json(new { success = true, message = "Jika email terdaftar, kode reset akan dikirim ke email Anda." });

            var token = new Random().Next(100000, 999999).ToString();

            admin.ResetToken = token;
            admin.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendForgotPasswordEmailAsync(
                    admin.Email,
                    token
                );
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal mengirim email: " + ex.Message });
            }

            return Json(new { success = true, message = "Kode reset password telah dikirim ke email Anda." });
        }

        // ================= GET AKTIVASI =================
        [HttpGet]
        [Route("Admin/Login/ActivateAccount")]
        public async Task<IActionResult> ActivateAccount(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Status  = "error";
                ViewBag.Message = "Token aktivasi tidak valid atau tidak ditemukan.";
                return View("~/Areas/Admin/Views/Aktivasi/Index.cshtml");
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.ActivationToken == token);

            if (admin == null)
            {
                ViewBag.Status  = "error";
                ViewBag.Message = "Token tidak ditemukan atau sudah pernah digunakan.";
                return View("~/Areas/Admin/Views/Aktivasi/Index.cshtml");
            }

            if (!admin.IsActive)
            {
                admin.IsActive = true;
                admin.ActivationToken = null;
                await _context.SaveChangesAsync();
            }

            ViewBag.Status  = "success";
            ViewBag.Message = "Akun berhasil diaktifkan! Silakan login.";

            return View("~/Areas/Admin/Views/Aktivasi/Index.cshtml");
        }

        // ================= POST AKTIVASI (Set Password) =================
        [HttpPost]
        [Route("Admin/Login/ActivateAccount")]
        public async Task<IActionResult> ActivateAccount(string token, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Json(new { success = false, message = "Token tidak valid." });

            if (string.IsNullOrWhiteSpace(password))
                return Json(new { success = false, message = "Password wajib diisi." });

            if (password.Length < 8)
                return Json(new { success = false, message = "Password minimal 8 karakter." });

            if (password != confirmPassword)
                return Json(new { success = false, message = "Konfirmasi password tidak cocok." });

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.ActivationToken == token);

            if (admin == null)
                return Json(new { success = false, message = "Token tidak valid atau sudah digunakan." });

            if (admin.IsActive)
                return Json(new { success = false, message = "Akun sudah aktif sebelumnya." });

            admin.PasswordHash    = _passwordHasher.HashPassword(admin, password);
            admin.IsActive        = true;
            admin.ActivationToken = null;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Akun berhasil diaktifkan! Silakan login." });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Hapus authentication cookie
            await HttpContext.SignOutAsync("AdminScheme");

            // Hapus session
            HttpContext.Session.Clear();

            // Hapus cookie auth manual
            Response.Cookies.Delete("SINTA_ADMIN_AUTH");

            // Disable cache browser
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Redirect ke login
            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }
    }
}