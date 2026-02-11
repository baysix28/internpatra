using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;
using System.Linq;
using System.Security.Claims;
using System.IO;

namespace sinta_asp.Controllers
{
    [Authorize]
    public class DashboardPesertaController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardPesertaController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;

            // Ambil data dari UserProfile (termasuk FotoProfil)
            var profil = await _context.UserProfile
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            var riwayatMagang = await _context.PendaftaranMagang
                .Where(m => m.EmailPribadi == userEmail)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.RiwayatMagang = riwayatMagang;
            
            // Ambil data Mahasiswa untuk nama profil
            var mhs = await _context.Mahasiswa.FirstOrDefaultAsync(m => m.Email == userEmail);
            ViewBag.NamaPeserta = mhs?.NamaLengkap ?? "Peserta SINTA";

            // Kirim model profil ke View agar @Model.FotoProfil tidak error
            return View(profil); 
        }

        [HttpGet]
        public IActionResult GetNotifications()
        {
            // Gunakan Set<Notification> untuk menghindari ambiguity
            var notifications = _context.Set<Notification>() 
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel {
                    Title = n.Title,
                    Message = n.Message,
                    IconClass = n.Type == "Email" ? "fa-envelope-open-text" : "fa-file-circle-check",
                    IconColor = n.Type == "Email" ? "text-primary" : "text-warning",
                    TimeAgo = CalculateTimeAgo(n.CreatedAt),
                    IsRead = n.IsRead
                }).ToList();

            return Json(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfil(string nama, string noHp, string univ)
        {
            try 
            {
                var userEmail = User.Identity?.Name;
                var profile = await _context.UserProfile.FirstOrDefaultAsync(m => m.Email == userEmail);

                if (profile != null)
                {
                    profile.NamaLengkap = nama;
                    profile.NoHP = noHp;
                    profile.NamaPerguruanTinggi = univ;
                    profile.UpdatedAt = DateTime.Now;
                    _context.UserProfile.Update(profile);
                }
                else 
                {
                    var newProfile = new UserProfile {
                        Email = userEmail ?? "",
                        NamaLengkap = nama,
                        NoHP = noHp,
                        NamaPerguruanTinggi = univ,
                        UserId = 0 
                    };
                    _context.UserProfile.Add(newProfile);
                }

                await _context.SaveChangesAsync(); 
                return Json(new { success = true, message = "Profil berhasil disimpan!" });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdatePassword(string oldPass, string newPass) // Pastikan nama ini sama dengan di AJAX
        {
            try 
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Json(new { success = false, message = "Sesi habis, silakan login ulang." });
                }

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null) 
                {
                    return Json(new { success = false, message = "User tidak ditemukan." });
                }

                // Cek apakah data dari AJAX masuk atau null
                if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
                {
                    return Json(new { success = false, message = "Data tidak diterima oleh server." });
                }

                if (user.Password.Trim() != oldPass.Trim())
                {
                    return Json(new { success = false, message = "Password lama salah!" });
                }

                user.Password = newPass.Trim();
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Ini akan membantu kamu melihat error apa yang terjadi di server via Debugger
                return Json(new { success = false, message = "Error Server: " + ex.Message });
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> UpdateFoto(IFormFile fotoProfil)
        {
            if (fotoProfil == null || fotoProfil.Length == 0)
                return Json(new { success = false, message = "File tidak ditemukan" });

            try
            {
                var userEmail = User.Identity?.Name;
                
                // 1. Tentukan folder (Gunakan Path.Combine agar aman)
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // 2. Nama File Unik
                string extension = Path.GetExtension(fotoProfil.FileName);
                string fileName = $"profile_{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(folderPath, fileName);

                // 3. Simpan File Fisik
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await fotoProfil.CopyToAsync(stream);
                }

                // 4. Update Database
                var profile = await _context.UserProfile.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (profile != null) 
                {
                    // Hapus foto lama dari folder jika ada
                    if (!string.IsNullOrEmpty(profile.FotoProfil))
                    {
                        string oldPath = Path.Combine(folderPath, profile.FotoProfil);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    profile.FotoProfil = fileName;
                    _context.UserProfile.Update(profile);
                    await _context.SaveChangesAsync();
                }

                // Return path lengkap untuk preview di Frontend
                return Json(new { success = true, fileName = fileName, filePath = "/uploads/profile/" + fileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFoto()
        {
            var userEmail = User.Identity?.Name;
            var profile = await _context.UserProfile.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (profile != null && !string.IsNullOrEmpty(profile.FotoProfil))
            {
                // Hapus file fisik
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile", profile.FotoProfil);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                profile.FotoProfil = null;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        private string CalculateTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalMinutes < 1) return "Baru saja";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} menit lalu";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} jam lalu";
            return dt.ToString("dd MMM yyyy");
        }

        public IActionResult Berhasil() => View();
    }
}