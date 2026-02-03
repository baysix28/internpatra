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
        public async Task<IActionResult> UpdateFoto(IFormFile fotoProfil)
        {
            if (fotoProfil == null || fotoProfil.Length == 0)
                return Json(new { success = false, message = "File tidak ditemukan" });

            try
            {
                var userEmail = User.Identity?.Name;
                
                // 1. Tentukan folder
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/profile");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // 2. Nama File Unik
                string extension = Path.GetExtension(fotoProfil.FileName);
                string fileName = $"profile_{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(folderPath, fileName);

                // 3. Simpan File
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await fotoProfil.CopyToAsync(stream);
                }

                // 4. UPDATE KE USERPROFILE (Bukan ke tabel Users)
                var profile = await _context.UserProfile.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (profile != null) 
                {
                    profile.FotoProfil = fileName;
                    _context.UserProfile.Update(profile);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, fileName = fileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdatePassword(string oldPass, string newPass)
        {
            // 1. Ambil ID dalam bentuk string
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Konversi string ke int (Ini kuncinya!)
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Sesi tidak valid." });
            }

            // 3. Sekarang bandingkan int dengan int
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            if (user == null) 
            {
                return Json(new { success = false, message = "User tidak ditemukan" });
            }

            // 4. Cek Password (Gunakan .Trim() untuk jaga-jaga spasi)
            if (user.Password.Trim() != oldPass.Trim())
            {
                return Json(new { success = false, message = "Password lama tidak sesuai!" });
            }

            // 5. Update dan Simpan
            user.Password = newPass.Trim();
            _context.SaveChanges();

            return Json(new { success = true });
        }
    

        [HttpPost]
        public async Task<IActionResult> DeleteFoto()
        {
            var userEmail = User.Identity?.Name;
            var profile = await _context.UserProfile.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (profile != null)
            {
                profile.FotoProfil = null;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        public IActionResult Berhasil() => View();
    }
}