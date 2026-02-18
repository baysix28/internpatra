using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;

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

        // ===== API SEARCH MONITORING =====
        [HttpGet]
        public async Task<IActionResult> CekStatus(string no)
        {
            if (string.IsNullOrEmpty(no))
                return Json(null);

            var data = await _context.PendaftaranMagang
                .Where(x => x.NoPendaftaran == no)
                .Select(x => new
                {
                    nama = x.NamaLengkap,
                    email = x.EmailPribadi,
                    status = x.Status,
                    nopendaftaran = x.NoPendaftaran,
                    posisi = x.Region
                })
                .FirstOrDefaultAsync();

            return Json(data);
        }
    }
}
