using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using System.Globalization;

namespace sinta_asp.Controllers
{
    public class HasilPendaftaranController : Controller
    {
        private readonly AppDbContext _context;

        public HasilPendaftaranController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CekStatus(string no)
        {
            if (string.IsNullOrEmpty(no))
                return Json(null);

            var culture = new CultureInfo("id-ID");

            var data = await _context.PendaftaranMagang
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NoPendaftaran != null &&
                                          x.NoPendaftaran.ToLower() == no.ToLower().Trim());

            if (data == null) return Json(null);

            return Json(new {
                nama        = data.NamaLengkap,
                email       = data.EmailPribadi,
                universitas = data.NamaPerguruanTinggi ?? "-",
                jurusan     = data.Jurusan ?? "-",
                region      = data.Region ?? "-",
                lokasi      = data.Lokasi ?? "-",
                mulai       = data.MulaiMagang.ToString("dd MMMM yyyy", culture),
                selesai     = data.SelesaiMagang.ToString("dd MMMM yyyy", culture),
                status      = data.Status
            });
        }

        [HttpGet]
        public async Task<IActionResult> CekStatusPenelitian(string no)
        {
            if (string.IsNullOrEmpty(no))
                return Json(null);

            var culture = new CultureInfo("id-ID");

            var data = await _context.Pendaftarans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NomorPendaftaran != null &&
                                          x.NomorPendaftaran.ToLower() == no.ToLower().Trim());

            if (data == null) return Json(null);

            // Normalisasi status tampilan
            var statusTampil = data.Status == "Dalam Proses" ? "Menunggu" : data.Status;

            return Json(new {
                nama             = data.Nama,
                email            = data.Email,
                universitas      = data.Universitas ?? "-",
                jurusan          = data.Jurusan ?? "-",
                judulPenelitian  = data.JudulPenelitian ?? "-",
                lokasiPenelitian = data.LokasiPenelitian ?? "-",
                region           = data.Region ?? "-",
                mulai            = data.TglMulai.HasValue
                                    ? data.TglMulai.Value.ToString("dd MMMM yyyy", culture)
                                    : "-",
                selesai          = data.TglSelesai.HasValue
                                    ? data.TglSelesai.Value.ToString("dd MMMM yyyy", culture)
                                    : "-",
                status           = statusTampil
            });
        }
    }
}