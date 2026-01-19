using Microsoft.AspNetCore.Mvc;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.EntityFrameworkCore;

namespace sinta_asp.Controllers
{
    public class PendaftaranMagangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // 🔥 Constructor
        public PendaftaranMagangController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ================= TEST DATABASE =================
        public IActionResult TestDb()
        {
            return Content(_context.Database.CanConnect().ToString());
        }
        // =================================================

        // GET: /PendaftaranMagang
        public IActionResult Index()
        {
            return View();
        }

        // GET: /PendaftaranMagang/DataDiri
        public IActionResult DataDiri()
        {
            return View();
        }

        // =================================================
        // POST: /PendaftaranMagang/Store
        // =================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Store(PendaftaranRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("DataDiri", request);
            }

            try
            {
                // ================= FOLDER UPLOAD =================
                string fotoPath = SimpanFile(request.Foto, "uploads/foto");
                string cvPath = SimpanFile(request.FileCV, "uploads/cv");
                string suratPath = SimpanFile(request.FileSuratPengantar, "uploads/surat");
                string proposalPath = SimpanFile(request.FileProposal, "uploads/proposal");

                // ================= SIMPAN KE DATABASE =================
                var magang = new Magang
                {
                    FotoProfil = fotoPath,
                    NamaLengkap = request.NamaLengkap!,
                    EmailPribadi = request.EmailPribadi!,
                    TempatLahir = request.TempatLahir,
                    TanggalLahir = request.TanggalLahir!.Value,
                    NoHp = request.NoHp,
                    Instagram = request.Instagram,

                    NamaPerguruanTinggi = request.NamaPerguruanTinggi,
                    Fakultas = request.Fakultas,
                    Jurusan = request.Jurusan,
                    Nim = request.NIM,

                    Company = request.Company,
                    Region = request.Region,
                    Lokasi = request.Lokasi,
                    RekomendasiPegawai = request.RekomendasiPegawai,

                    MulaiMagang = request.MulaiMagang!.Value,
                    SelesaiMagang = request.SelesaiMagang!.Value,

                    FileCv = cvPath,
                    FileSuratPengantar = suratPath,
                    FileProposal = proposalPath,
                    CreatedAt = DateTime.Now
                };

                _context.PendaftaranMagang.Add(magang);
                _context.SaveChanges();

                return RedirectToAction("Sukses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Terjadi kesalahan: " + ex.Message;
                return View("DataDiri", request);
            }
        }

        // ================= HELPER UPLOAD FILE =================
        private string SimpanFile(IFormFile? file, string folder)
        {
            if (file == null) return "";

            string uploadDir = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // path yang disimpan ke database
            return $"{folder}/{fileName}";
        }

        // =================================================
        public IActionResult Sukses()
        {
            return View();
        }

        public IActionResult Hasil()
        {
            return View();
        }

        // 🔥 HALAMAN LIHAT DATA (ADMIN)
        public IActionResult DataMagang()
        {
            var data = _context.PendaftaranMagang
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(data);
        }
    }
}
