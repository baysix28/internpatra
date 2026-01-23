using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MahasiswaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MahasiswaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Mahasiswa
        public async Task<IActionResult> Index()
        {
            var mahasiswa = await _context.Mahasiswa.ToListAsync();
            return View(mahasiswa);
        }

        // GET: Admin/Mahasiswa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mahasiswa = await _context.Mahasiswa
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mahasiswa == null)
            {
                return NotFound();
            }

            return View(mahasiswa);
        }

        // GET: Admin/Mahasiswa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mahasiswa = await _context.Mahasiswa.FindAsync(id);
            if (mahasiswa == null)
            {
                return NotFound();
            }
            return View(mahasiswa);
        }

        // POST: Admin/Mahasiswa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TipePendaftaran,FotoPath,NamaLengkap,Email,TempatLahir,TanggalLahir,NoHP,Instagram,NamaKampus,Fakultas,Jurusan,NIM,Company,Region,Lokasi,DetailTambahan,TanggalMulai,TanggalSelesai,CVPath,ProposalPath,SuratKampusPath,Status")] Mahasiswa mahasiswa)
        {
            if (id != mahasiswa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mahasiswa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MahasiswaExists(mahasiswa.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mahasiswa);
        }

        // POST: Admin/Mahasiswa/ChangeStatus
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var mahasiswa = await _context.Mahasiswa.FindAsync(id);
            if (mahasiswa == null)
            {
                return Json(new { success = false, message = "Data tidak ditemukan" });
            }

            mahasiswa.Status = status;
            _context.Update(mahasiswa);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Status berhasil diubah" });
        }

        // POST: Admin/Mahasiswa/BulkApprove
        [HttpPost]
        public async Task<IActionResult> BulkApprove([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Tidak ada data yang dipilih" });
            }

            var mahasiswaList = await _context.Mahasiswa
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();

            foreach (var mahasiswa in mahasiswaList)
            {
                mahasiswa.Status = "Diterima";
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"{mahasiswaList.Count} data berhasil diterima" });
        }

        // POST: Admin/Mahasiswa/BulkReject
        [HttpPost]
        public async Task<IActionResult> BulkReject([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Tidak ada data yang dipilih" });
            }

            var mahasiswaList = await _context.Mahasiswa
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();

            foreach (var mahasiswa in mahasiswaList)
            {
                mahasiswa.Status = "Ditolak";
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"{mahasiswaList.Count} data berhasil ditolak" });
        }

        // POST: Admin/Mahasiswa/BulkSendEmail
        [HttpPost]
        public async Task<IActionResult> BulkSendEmail([FromBody] BulkEmailRequest request)
        {
            if (request == null || request.Ids == null || !request.Ids.Any())
            {
                return Json(new { success = false, message = "Tidak ada data yang dipilih" });
            }

            var mahasiswaList = await _context.Mahasiswa
                .Where(m => request.Ids.Contains(m.Id))
                .Select(m => m.Email)
                .ToListAsync();

            // TODO: Implement email sending logic here
            // This is just a placeholder - implement actual email sending
            
            return Json(new { 
                success = true, 
                message = $"Email akan dikirim ke {mahasiswaList.Count} penerima",
                count = mahasiswaList.Count
            });
        }

        // POST: Admin/Mahasiswa/SendEmail
        [HttpPost]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Message))
            {
                return Json(new { success = false, message = "Data email tidak lengkap" });
            }

            // TODO: Implement email sending logic here
            // This is just a placeholder - implement actual email sending
            
            return Json(new { 
                success = true, 
                message = "Email berhasil dikirim"
            });
        }

        // POST: Admin/Mahasiswa/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var mahasiswa = await _context.Mahasiswa.FindAsync(id);
            if (mahasiswa == null)
            {
                return Json(new { success = false, message = "Data tidak ditemukan" });
            }

            _context.Mahasiswa.Remove(mahasiswa);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Data berhasil dihapus" });
        }

        // POST: Admin/Mahasiswa/Import
        [HttpPost]
        public async Task<IActionResult> Import([FromBody] List<MahasiswaImportDto> data)
        {
            if (data == null || !data.Any())
            {
                return Json(new { success = false, message = "Tidak ada data untuk diimport" });
            }

            try
            {
                var mahasiswaList = new List<Mahasiswa>();
                
                foreach (var item in data)
                {
                    var mahasiswa = new Mahasiswa
                    {
                        NamaLengkap = item.NamaLengkap ?? "",
                        Email = item.Email ?? "",
                        NIM = item.NIM ?? "",
                        NamaKampus = item.NamaKampus ?? "",
                        Fakultas = item.Fakultas ?? "",
                        Jurusan = item.Jurusan ?? "",
                        TipePendaftaran = item.TipePendaftaran ?? "Magang",
                        Status = "Pending",
                        Company = item.Company ?? "",
                        Lokasi = item.Lokasi ?? "",
                        NoHP = item.NoHP ?? "",
                        TanggalMulai = item.TanggalMulai,
                        TanggalSelesai = item.TanggalSelesai
                    };
                    
                    mahasiswaList.Add(mahasiswa);
                }

                await _context.Mahasiswa.AddRangeAsync(mahasiswaList);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"{mahasiswaList.Count} data berhasil diimport" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error: {ex.Message}" 
                });
            }
        }

        private bool MahasiswaExists(int id)
        {
            return _context.Mahasiswa.Any(e => e.Id == id);
        }
    }

    // DTO Classes
    public class BulkEmailRequest
    {
        public List<int> Ids { get; set; }
        public string Message { get; set; }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }

    public class MahasiswaImportDto
    {
        public string NamaLengkap { get; set; }
        public string Email { get; set; }
        public string NIM { get; set; }
        public string NamaKampus { get; set; }
        public string Fakultas { get; set; }
        public string Jurusan { get; set; }
        public string TipePendaftaran { get; set; }
        public string Company { get; set; }
        public string Lokasi { get; set; }
        public string NoHP { get; set; }
        public DateTime? TanggalMulai { get; set; }
        public DateTime? TanggalSelesai { get; set; }
    }
}