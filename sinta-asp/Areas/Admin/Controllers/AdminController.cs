using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using sinta_asp.Data;
using sinta_asp.Models;
using System.Threading.Tasks;
using System.Linq;

// ALIAS MODEL ADMIN
using AdminModel = sinta_asp.Models.Admin;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<AdminModel> _passwordHasher;

        public AdminsController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AdminModel>();
        }

        // ==========================================
        // MIDDLEWARE SEDERHANA: CEK APAKAH SUPER ADMIN
        // ==========================================
        private bool IsSuperAdmin()
        {
            return HttpContext.Session.GetString("AdminRole") == "SuperAdmin";
        }

        // ===============================
        // LIST SEMUA ADMIN
        // ===============================
        public async Task<IActionResult> Index()
        {
            // Jika bukan Super Admin, langsung lempar ke Dashboard tanpa pesan error
            if (!IsSuperAdmin()) return RedirectToAction("Index", "Dashboard");

            var listAdmin = await _context.Admins.OrderBy(a => a.Role).ToListAsync();
            return View(listAdmin);
        }

        // ===============================
        // CREATE: TAMBAH ADMIN BARU
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Create(AdminModel model, string NewPassword)
        {
            // Proteksi silent untuk request POST
            if (!IsSuperAdmin()) return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrEmpty(NewPassword))
                return Json(new { success = false, message = "Password wajib diisi" });

            var existing = await _context.Admins.AnyAsync(a => a.Email == model.Email);
            if (existing)
                return Json(new { success = false, message = "Email sudah terdaftar" });

            model.PasswordHash = _passwordHasher.HashPassword(model, NewPassword);
            
            _context.Admins.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Akun Admin Region berhasil dibuat" });
        }

        // ===============================
        // EDIT: UPDATE DATA ADMIN
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Edit(AdminModel model, string NewPassword)
        {
            if (!IsSuperAdmin()) return RedirectToAction("Index", "Dashboard");

            var admin = await _context.Admins.FindAsync(model.Id);
            if (admin == null) return Json(new { success = false, message = "Data tidak ditemukan" });

            admin.Nama = model.Nama;
            admin.Email = model.Email;
            admin.Role = model.Role;
            admin.RegionManaged = model.RegionManaged;
            admin.SmtpPassword = model.SmtpPassword;

            if (!string.IsNullOrEmpty(NewPassword))
            {
                admin.PasswordHash = _passwordHasher.HashPassword(admin, NewPassword);
            }

            _context.Update(admin);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Data admin berhasil diperbarui" });
        }

        // ===============================
        // DELETE: HAPUS ADMIN
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsSuperAdmin()) return RedirectToAction("Index", "Dashboard");

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return Json(new { success = false, message = "Data tidak ditemukan" });

            // Mencegah menghapus diri sendiri
            var currentId = HttpContext.Session.GetString("AdminId");
            if (id.ToString() == currentId)
                return Json(new { success = false, message = "Anda tidak bisa menghapus akun sendiri" });

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Akun berhasil dihapus" });
        }
    }
}