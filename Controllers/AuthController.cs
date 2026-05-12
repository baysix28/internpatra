using Microsoft.AspNetCore.Mvc;
using sinta_asp.Data;
using sinta_asp.Models;
using sinta_asp.Services; // 1. TAMBAHKAN INI
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace sinta_asp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService; // 2. TAMBAHKAN INI

        // Update Constructor untuk menerima emailService
        public AuthController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService; // 2. TAMBAHKAN INI
        }

        [HttpGet]
        public IActionResult Login() => View();

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("PesertaScheme");
            return RedirectToAction("Index", "Home"); 
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password, string? returnUrl = null)
        {
            var account = _context.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (account != null)
            {
                if (!account.IsEmailConfirmed)
                {
                    TempData["ErrorMessage"] = "Akun Anda belum aktif. Harap verifikasi email Anda!";
                    return RedirectToAction("Login");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Email),
                    new Claim("Id", account.Id.ToString()),
                    new Claim(ClaimTypes.Role, account.Role ?? "Peserta")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "PesertaScheme");

                await HttpContext.SignInAsync(
                    "PesertaScheme",
                    new ClaimsPrincipal(claimsIdentity));

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "DashboardPeserta");
            }

            TempData["ErrorMessage"] = "Email atau Password salah!";
            return RedirectToAction("Login");
        }
        // --- FITUR LUPA PASSWORD ---

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                // Tetap tampilkan pesan sukses demi keamanan agar hacker tidak tahu email mana yang terdaftar
                TempData["SuccessMessage"] = "Jika email terdaftar, instruksi reset akan dikirim ke email Anda.";
                return RedirectToAction("Login");
            }

            // Buat token unik (bisa pakai VerificationToken yang sudah ada di model)
            user.VerificationToken = Guid.NewGuid().ToString();
            _context.SaveChanges();

            var resetLink = Url.Action("ResetPassword", "Auth", new { token = user.VerificationToken, email = user.Email }, Request.Scheme);

            string subject = "Reset Password SINTA Pertamina";
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; border: 1px solid #ddd; padding: 20px;'>
                    <h2 style='color: #00549B;'>Permintaan Reset Password</h2>
                    <p>Kami menerima permintaan untuk mengatur ulang password akun SINTA Anda.</p>
                    <p>Klik tombol di bawah ini untuk membuat password baru:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #00549B; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>RESET PASSWORD</a>
                    </div>
                    <p>Jika Anda tidak merasa melakukan permintaan ini, abaikan email ini.</p>
                    <hr>
                    <p style='font-size: 12px; color: #888;'>SINTA Pertamina - Sinergi Teknologi Adaptif</p>
                </div>";

            await _emailService.SendWithCourierAsync(email, subject, body, "SINTA Pertamina");

            TempData["SuccessMessage"] = "Instruksi reset password telah dikirim ke email Anda.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.VerificationToken == token);
            if (user == null) return Content("Link tidak valid atau kadaluarsa.");
            
            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string token, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.VerificationToken == token);
            if (user != null)
            {
                user.Password = newPassword; // Pastikan nanti di-hash jika sudah pakai hashing
                user.VerificationToken = null;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Password berhasil diperbarui! Silakan login.";
                return RedirectToAction("Login");
            }
            return Content("Terjadi kesalahan.");
        }
        [HttpGet]
        public IActionResult Register() => View();

        // 3. UBAH BAGIAN INI (Tambah async Task dan Logic Kirim Email)
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                model.IsEmailConfirmed = false; 
                model.VerificationToken = Guid.NewGuid().ToString();

                _context.Users.Add(model);
                _context.SaveChanges();

                // --- PROSES KIRIM EMAIL ---
                var callbackUrl = Url.Action("ConfirmEmail", "Auth", 
                    new { token = model.VerificationToken, email = model.Email }, Request.Scheme);

                string subject = "Verifikasi Akun SINTA Pertamina";
                string body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; border: 1px solid #ddd; padding: 20px;'>
                        <h2 style='color: #00549B;'>Halo {model.Nama}!</h2>
                        <p>Terima kasih telah mendaftar di aplikasi SINTA Pertamina.</p>
                        <p>Klik tombol di bawah ini untuk mengaktifkan akun Anda:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{callbackUrl}' style='background-color: #E30613; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>AKTIFKAN AKUN SAYA</a>
                        </div>
                        <p>Jika tombol tidak berfungsi, silakan salin link berikut: <br> {callbackUrl}</p>
                        <hr>
                        <p style='font-size: 12px; color: #888;'>Ini adalah email otomatis, mohon jangan dibalas.</p>
                    </div>";

                try 
                {
                    // Memanggil service email yang sudah kamu buat sebelumnya
                    await _emailService.SendWithCourierAsync(model.Email, subject, body, "SINTA Pertamina");
                    TempData["SuccessMessage"] = "Registrasi Berhasil! Silakan cek EMAIL Anda untuk melakukan verifikasi.";
                }
                catch (Exception ex)
                {
                    // Jika email gagal terkirim (misal internet mati atau pass smtp salah)
                    TempData["ErrorMessage"] = "User tersimpan, tapi gagal mengirim email verifikasi. Error: " + ex.Message;
                }
                
                return RedirectToAction("Login");
            }
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.VerificationToken == token);
            
            if (user == null) {
                return Content("Link verifikasi tidak valid atau sudah kadaluarsa.");
            }

            user.IsEmailConfirmed = true;
            user.VerificationToken = null; 
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Email berhasil diverifikasi! Sekarang Anda bisa login.";
            return RedirectToAction("Login");
        }
    }
}