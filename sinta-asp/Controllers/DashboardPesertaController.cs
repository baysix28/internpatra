using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data; // Pastikan namespace ini sesuai folder Data kamu
using sinta_asp.Models;
using System.Linq;
using System.Security.Claims;

namespace sinta_asp.Controllers
{
    [Authorize]
    public class DashboardPesertaController : Controller
    {
        private readonly AppDbContext _context;

        // Gunakan Dependency Injection untuk memanggil AppDbContext
        public DashboardPesertaController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;

            // Pakai "_context.Magangs" (sesuaikan dengan nama DbSet di AppDbContext kamu)
            // Dan pastikan filter emailnya benar
            var riwayatMagang = await _context.PendaftaranMagang
                .Where(m => m.EmailPribadi == userEmail)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.RiwayatMagang = riwayatMagang;
            
            // Ambil data Mahasiswa untuk nama profil
            var mhs = await _context.Mahasiswa.FirstOrDefaultAsync(m => m.Email == userEmail);
            ViewBag.NamaPeserta = mhs?.NamaLengkap ?? "Peserta SINTA";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DaftarMagang(Magang model) // Tetap gunakan Magang
        {
            if (ModelState.IsValid)
            {
                // Karena di model tidak ada kolom 'Status', pastikan di Database 
                // kamu sudah menjalankan migrasi 'AddStatusToTable'
                // Jika kolom Status ada di DB tapi tidak di model, tambahkan di Magang.cs:
                // public string Status { get; set; } = "Proses Review";

                model.CreatedAt = DateTime.Now; // Gunakan CreatedAt

                _context.PendaftaranMagang.Add(model); // Simpan ke DbSet yang benar
                await _context.SaveChangesAsync(); 
                
                return RedirectToAction("Berhasil");
            }
            return View(model);
        }

        public IActionResult Berhasil()
        {
            return View();
        }
    }
}