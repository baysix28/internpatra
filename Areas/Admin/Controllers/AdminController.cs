using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using sinta_asp.Data;
using sinta_asp.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System;
using Microsoft.AspNetCore.Authorization;

using AdminModel = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]
    public class AdminsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;
        private readonly IConfiguration _config;

        public AdminsController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _passwordHasher = new PasswordHasher<AdminModel>();
        }

        // ✅ DIUBAH: SuperAdmin ATAU Admin Jawa Tengah juga dapat akses
        private bool IsUserAuthorized()
        {
            var adminRole   = HttpContext.Session.GetString("AdminRole")?.Trim();
            var adminRegion = HttpContext.Session.GetString("AdminRegion")?.Trim();

            bool isSuperAdmin = string.Equals(adminRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
            bool isJawaTengah = string.Equals(adminRegion, "Regional Jawa Bagian Tengah", StringComparison.OrdinalIgnoreCase);

            return isSuperAdmin || isJawaTengah;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsUserAuthorized()) return RedirectToAction("Index", "Dashboard");
            var listAdmin = await _context.Admins.OrderBy(a => a.Role).ToListAsync();
            return View(listAdmin);
        }

        public IActionResult Success()
        {
            return View("~/Areas/Admin/Views/Aktivasi/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminModel model, string NewPassword, string Region)
        {
            if (!IsUserAuthorized()) return Json(new { success = false, message = "Akses ditolak" });

            if (string.IsNullOrEmpty(NewPassword))
                return Json(new { success = false, message = "Password wajib diisi" });

            // CEK DUPLIKASI EMAIL
            var existing = await _context.Admins.FirstOrDefaultAsync(a => a.Email == model.Email);
            if (existing != null)
            {
                if (!existing.IsActive)
                {
                    try
                    {
                        await SendActivationEmail(existing);
                        return Json(new { success = true, message = "Email aktivasi telah dikirim ulang ke alamat tersebut." });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Gagal mengirim ulang email: " + ex.Message });
                    }
                }
                return Json(new { success = false, message = "Email sudah terdaftar dan sudah aktif." });
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    model.Region = Region ?? model.Region;
                    model.PasswordHash = _passwordHasher.HashPassword(model, NewPassword);
                    model.CreatedAt = DateTime.Now;

                    if (model.Role == "SuperAdmin")
                    {
                        model.IsActive = true;
                        model.ActivationToken = null;
                    }
                    else
                    {
                        model.IsActive = false;
                        model.ActivationToken = Guid.NewGuid().ToString();
                    }

                    _context.Admins.Add(model);
                    await _context.SaveChangesAsync();

                    if (!model.IsActive)
                    {
                        await SendActivationEmail(model);
                    }

                    await transaction.CommitAsync();

                    string successMsg = model.IsActive
                        ? "Akun berhasil dibuat dan langsung aktif."
                        : "Akun berhasil dibuat. Instruksi aktivasi telah dikirim ke " + model.Email;

                    return Json(new { success = true, message = successMsg });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Gagal mendaftar: " + (ex.InnerException?.Message ?? ex.Message) });
                }
            });
        }

        private async Task SendActivationEmail(AdminModel admin)
        {
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var senderEmail = _config["EmailSettings:Email"];
            var senderPass = _config["EmailSettings:Password"];

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPass))
                throw new Exception("Konfigurasi SMTP di appsettings.json belum lengkap.");

            var activationLink = $"{Request.Scheme}://{Request.Host}/Admin/Login/ActivateAccount?token={admin.ActivationToken}";

            var message = new MailMessage();
            message.To.Add(new MailAddress(admin.Email));
            message.From = new MailAddress(senderEmail, "SINTA Pertamina");
            message.Subject = "Aktivasi Akun Administrator SINTA";
            message.Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background: #ffffff; padding: 30px; border-radius: 12px; border: 1px solid #e2e8f0;'>
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <h2 style='color: #00549B; margin: 0;'>Aktivasi Akun Admin</h2>
                            <p style='color: #64748b;'>Sistem Informasi Terintegrasi Asset (SINTA)</p>
                        </div>
                        <hr style='border: 0; border-top: 1px solid #eee;'>
                        <p>Halo <strong>{admin.Nama}</strong>,</p>
                        <p>Anda telah didaftarkan sebagai administrator sistem. Demi keamanan, silakan aktifkan akun Anda melalui tombol di bawah ini:</p>
                        <div style='text-align: center; margin: 35px 0;'>
                            <a href='{activationLink}' style='background-color: #00549B; color: #ffffff; padding: 14px 35px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;'>Aktifkan Akun Sekarang</a>
                        </div>
                        <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Jika tombol tidak berfungsi, salin link berikut:<br>{activationLink}</p>
                        <hr style='border: 0; border-top: 1px solid #eee; margin-top: 30px;'>
                        <p style='font-size: 11px; color: #cbd5e1; text-align: center;'>Tim IT SINTA Pertamina</p>
                    </div>
                </body>
                </html>";
            message.IsBodyHtml = true;

            using var client = new SmtpClient(host, port);
            client.Credentials = new NetworkCredential(senderEmail, senderPass);
            client.EnableSsl = true;
            client.Timeout = 20000;
            await client.SendMailAsync(message);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminModel model, string NewPassword, string Region)
        {
            if (!IsUserAuthorized()) return Json(new { success = false });

            var admin = await _context.Admins.FindAsync(model.Id);
            if (admin == null) return Json(new { success = false, message = "Data tidak ditemukan" });

            try
            {
                admin.Nama = model.Nama;
                admin.Email = model.Email;
                admin.Role = model.Role;
                admin.Region = Region ?? model.Region;
                admin.SmtpPassword = model.SmtpPassword;

                if (!string.IsNullOrEmpty(NewPassword))
                    admin.PasswordHash = _passwordHasher.HashPassword(admin, NewPassword);

                _context.Update(admin);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Data admin berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal update: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsUserAuthorized()) return Json(new { success = false });

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return Json(new { success = false, message = "Data tidak ditemukan" });

            var currentId = HttpContext.Session.GetString("AdminId");
            if (id.ToString() == currentId)
                return Json(new { success = false, message = "Anda tidak bisa menghapus akun Anda sendiri" });

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Akun berhasil dihapus permanent" });
        }
    }
}