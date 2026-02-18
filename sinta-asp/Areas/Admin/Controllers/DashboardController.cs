using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;
using sinta_asp.Areas.Admin.Models;
using System.Globalization;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        // Master Data Unit & Region (Sumber utama Dropdown & Chart)
        private readonly Dictionary<string, List<string>> _masterDataUnit = new Dictionary<string, List<string>>
        {
            { "Refinery Unit VI Balongan", new List<string> { 
                "Akuntansi / Ekonomi & Bisnis", "Elektro (Arus Kuat)", "Elektro (Arus Lemah)", "Emergency & Insurance", "Health", "Hukum", 
                "Ilmu Komunikasi / FISIP / Administrasi Publik", "Internal Audit", "Kelautan / Perkapalan", "Kimia Murni / MIPA", 
                "Konversi Energi / Migas / Kimia Air Bersih / Blanding / Loading", "Logistik / Pergudangan / Procurement", 
                "Manajemen / SDM / Psikologi", "Metalurgi / Material / Dirgantara", "Safety (K3) / SMK3", "Teknik Fisika", 
                "Teknik Industri", "Teknik Informatika", "Teknik Kimia", "Teknik Lingkungan", "Teknik Mesin", "Teknik Mesin (Rotating)", "Teknik Sipil" 
            } },
            { "Regional Jatimbalinus", new List<string> { 
                "Asset Operation MOR V","Bitumen Plant Gresik","C&T IA Jatimbalinus","Comm, Rel, & CSR MOR V",
                "Corporate Operation & Service Region V","Corporate Sales Region V","DPPU BIL","DPPU Eltari Group",
                "DPPU Iswahyudi","DPPU Juanda","DPPU Ngurah Rai","Finance MOR V","Fuel Terminal Atapupu",
                "Fuel Terminal Badas","Fuel Terminal Bima","Fuel Terminal Camplong","Fuel Terminal Ende",
                "Fuel Terminal Kalabahi","Fuel Terminal Madiun","Fuel Terminal Malang","Fuel Terminal Maumere",
                "Fuel Terminal Reo","Fuel Terminal Sanggaran","Fuel Terminal Tenau","Fuel Terminal Tuban",
                "Fuel Terminal Waingapu","HC Jatimbalinus","HSSE Region V","Integrated Terminal Ampenan",
                "Integrated Terminal Manggis","Integrated Terminal Surabaya","Integrated Terminal T. Wangi",
                "Legal Counsel Regional Jatimbalinus","Marine Region V","Medical Jatimbalinus",
                "Procurement MOR V","Rel & Project Dev Region V","Retail Bali","Retail Kediri",
                "Retail Malang","Retail NTB","Retail NTT","Retail Sales Region V","Retail Surabaya",
                "S&D Region V","SSC ICT VI Jatimbalinus"
            } },
            { "Regional Jawa Bagian Barat", new List<string> { 
                "Asset Operation JBB","Corp. Opt & Serv JBB","Corporate Sales JBB","DPPU Halim PK Group",
                "DPPU Husein Sastranegara","DPPU Kertajati","Finance JBB","Fuel Terminal Bandung Group",
                "Fuel Terminal Cikampek","Fuel Terminal Tasikmalaya","Fuel Terminal Tg Gerem","HSSE JBB",
                "Human Capital","Integrated Terminal Balongan","Integrated Terminal Jakarta","Legal Counsel JBB",
                "Medical JBB","MWH & LPG Cylinder","Procurement JBB","Reliability & Project Dev JBB",
                "SA Retail Bandung","SA Retail Cirebon","SA Retail Karawang","SA Retail Sukabumi",
                "SAM Retail Banten","SAM Retail Jabode","SHAFTHI","SHIPS","SCC ICT JBB",
                "Supply & Distribution JBB","Unit Comm, Rel & CSR JBB"
            } },
            { "Regional Jawa Bagian Tengah", new List<string> { 
                "AFT Adi Sumarmo","AFT Adi Sucipto","AFT Ahmad Yani","AFT YIA",
                "Fuel Terminal Boyolali","Fuel Terminal Lomanis","Fuel Terminal Maos",
                "Fuel Terminal Rewulu","Fuel Terminal Tegal","Integrated Terminal Cilacap",
                "Integrated Terminal Semarang","Kantor Branch Marketing DIY & Surakarta",
                "Kantor Unit - Asset Operation JBT","Kantor Unit - Comm, Rel & CSR JBT",
                "Kantor Unit - Corp Operation & Serv JBT","Kantor Unit - Corporate Sales JBT",
                "Kantor Unit - Finance JBT","Kantor Unit - HC JBT","Kantor Unit - HSSE JBT",
                "Kantor Unit - Internal Audit","Kantor Unit - Legal Counsel JBT",
                "Kantor Unit - Medical JBT","Kantor Unit - Operational Risk JBT",
                "Kantor Unit - Procurement JBT","Kantor Unit - Rel & Project Dev JBT",
                "Kantor Unit - Retail Sales JBT","Kantor Unit - SSC ICT V JBT",
                "Kantor Unit - Supply & Distribution JBT"
            } },
            { "Regional Kalimantan", new List<string> { 
                "DPPU APT Pranoto","DPPU H. Asan","DPPU Iskandar","DPPU Juwata",
                "DPPU Kalimaru","DPPU Sepinggan","DPPU Supadio","DPPU Syamsudin Noor",
                "DPPU Tjilik Riwut","Fuel Terminal Pulang Pisau","Fuel Terminal Kotabaru",
                "Fuel Terminal Pangkalan Bun","Fuel Terminal Samarinda","Fuel Terminal Sampit",
                "Fuel Terminal Sintang","Fuel Terminal Tarakan","Integrated Terminal Balikpapan",
                "Integrated Terminal Banjarmasin","Integrated Terminal Pontianak",
                "Kantor Patra Niaga Region Kalimantan","SAM Retail Kalbar","SAM Retail Kalselteng",
                "SAM Retail Kaltimut"
            } },
            { "Regional Maluku Papua", new List<string> { 
                "Aviation FT Babullah","Aviation FT Deo","Aviation FT Depati Mopah",
                "Aviation FT Depati Rendani","Aviation FT Dumatubun","Aviation FT Frans Kaisiepo",
                "Aviation FT Mathilda","Aviation FT Mozes Kilangin","Aviation FT Paniai",
                "Aviation FT Pattimura","Aviation FT Sentani","Aviation FT Utarom",
                "FT Biak","FT Bula","FT Dobo","FT Fak-Fak","FT Kaimana","FT Labuha",
                "FT Manokwari","FT Masohi","FT Merauke","FT Nabire","FT Namlea",
                "FT Sanana","FT Saumlaki","FT Serui","FT Sorong","FT Ternate",
                "FT Tobelo","FT Tual","IT Jayapura","IT Wayame",
                "Kantor Region - Asset Operation Papua-Maluku",
                "Kantor Region - Comm, Rel & CSR Papua-Maluku",
                "Kantor Region - Corp Operation & Serv Papua-Maluku",
                "Kantor Region - Corporate Sales Papua-Maluku",
                "Kantor Region - Finance Papua-Maluku",
                "Kantor Region - HC Papua-Maluku",
                "Kantor Region - HSSE Papua-Maluku",
                "Kantor Region - Legal Counsel Papua-Maluku",
                "Kantor Region - Medical Papua-Maluku",
                "Kantor Region - Procurement Papua-Maluku",
                "Kantor Region - Rel & Project Dev Papua-Maluku",
                "Kantor Region - Retail Sales Papua-Maluku",
                "Kantor Region - Supply & Dist Papua-Maluku",
                "Sales Area Ambon"
            } },
            { "Regional Sumbagut", new List<string> { 
                "Asset Operation Region Sumbagut","Branch Marketing Aceh",
                "Branch Marketing Kepulauan Riau","Branch Marketing Sibolga",
                "Branch Marketing Sumbar","Communication & CSR Region Sumbagut",
                "Corp Operation & Serv Region Sumbagut","Corporate Sales Region Sumbagut",
                "DPPU Hang Nadim Group","DPPU Kualanamu Group","DPPU Minangkabau",
                "DPPU SIM","DPPU SSK II","Finance Region Sumbagut",
                "Fuel Terminal Batam","Fuel Terminal Gunung Sitoli",
                "Fuel Terminal Kijang Group","Fuel Terminal Kisaran",
                "Fuel Terminal Krueng Raya","Fuel Terminal Medan Group",
                "Fuel Terminal Meulaboh","Fuel Terminal Natuna Group",
                "Fuel Terminal Pematang Siantar","Fuel Terminal Sabang",
                "Fuel Terminal Sei Siak","Fuel Terminal Sibolga",
                "Fuel Terminal Simeulue","Fuel Terminal Tembilahan",
                "HC Region Sumbagut","HSSE Region Sumbagut",
                "IA Region I","Integrated Terminal Dumai",
                "Integrated Terminal Lhokseumawe","Integrated Terminal Tanjung Uban",
                "Integrated Terminal Teluk Kabung","Legal Counsel Region Sumbagut",
                "Medical Region Sumbagut","Procurement Region Sumbagut",
                "Rel & Project Dev Region Sumbagut",
                "Retail Sales Region Sumbagut",
                "SSC ICT I Region Sumbagut",
                "Supply & Distribution Region Sumbagut"
            } }
        };

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? region)
        {
            var adminRole = HttpContext.Session.GetString("AdminRole") ?? "SuperAdmin";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion");
            var adminName = HttpContext.Session.GetString("AdminName");

            // --- REVISI LOGIKA DEFAULT REGION ---
            string activeRegion;
            if (!string.IsNullOrEmpty(region))
            {
                activeRegion = region;
            }
            else if (adminRole == "SuperAdmin")
            {
                activeRegion = "All";
            }
            else
            {
                activeRegion = sessionRegion ?? "All";
            }
            // --------------------------------------

            var allRegionsFromMaster = _masterDataUnit.Keys.OrderBy(x => x).ToList();
            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin")
            {
                // Admin biasa dipaksa hanya melihat regionnya sendiri
                query = query.Where(x => x.Region == sessionRegion);
                activeRegion = sessionRegion ?? "All";
            }
            else if (activeRegion != "All" && activeRegion != "Semua Region")
            {
                query = query.Where(x => x.Region == activeRegion);
            }

            var data = await query.ToListAsync();
            var now = DateTime.Now;
            var currentYear = now.Year;
            var idCulture = new CultureInfo("id-ID");

            var last7Days = Enumerable.Range(0, 7).Select(i => now.Date.AddDays(-i)).OrderBy(d => d).ToList();
            var months = Enumerable.Range(1, 12).ToList();
            var years = Enumerable.Range(2024, (currentYear - 2024) + 1).ToList();

            var model = new DashboardModel
            {
                AdminRole = adminRole,
                AdminRegion = activeRegion,
                AdminName = adminName,
                Regions = allRegionsFromMaster, 
                StatusDiproses = data.Count(x => x.Status == "Menunggu"),
                StatusDiterima = data.Count(x => x.Status == "Diterima"),
                StatusDitolak = data.Count(x => x.Status == "Ditolak"),
                WeeklyLabels = last7Days.Select(d => d.ToString("dddd", idCulture)).ToList(), 
                WeeklyCounts = last7Days.Select(d => data.Count(x => x.CreatedAt.Date == d)).ToList(),
                MonthlyLabels = months.Select(m => new DateTime(currentYear, m, 1).ToString("MMM", idCulture)).ToList(), 
                MonthlyCounts = months.Select(m => data.Count(x => x.CreatedAt.Month == m && x.CreatedAt.Year == currentYear)).ToList(),
                YearlyLabels = years.Select(y => y.ToString()).ToList(),
                YearlyCounts = years.Select(y => data.Count(x => x.CreatedAt.Year == y)).ToList(),
                KampusLabels = data.GroupBy(x => x.NamaPerguruanTinggi).OrderByDescending(x => x.Count()).Take(10).Select(x => x.Key ?? "N/A").ToList(),
                KampusCounts = data.GroupBy(x => x.NamaPerguruanTinggi).OrderByDescending(x => x.Count()).Take(10).Select(x => x.Count()).ToList(),
                DaftarMagang = data.OrderByDescending(x => x.CreatedAt).ToList()
            };

            if (activeRegion == "All" || activeRegion == "Semua Region")
            {
                model.LokasiStatLabels = allRegionsFromMaster;
                model.LokasiDiterima = model.LokasiStatLabels.Select(r => data.Count(x => x.Region == r && x.Status == "Diterima")).ToList();
                model.LokasiMenunggu = model.LokasiStatLabels.Select(r => data.Count(x => x.Region == r && x.Status == "Menunggu")).ToList();
                model.LokasiDitolak = model.LokasiStatLabels.Select(r => data.Count(x => x.Region == r && x.Status == "Ditolak")).ToList();
                ViewBag.SebaranTitle = "Rekap Sebaran Per Region";
                ViewBag.SubTitle = "Nasional";
            }
            else
            {
                model.LokasiStatLabels = _masterDataUnit.ContainsKey(activeRegion) ? _masterDataUnit[activeRegion] : new List<string>();
                model.LokasiDiterima = model.LokasiStatLabels.Select(u => data.Count(x => x.Lokasi == u && x.Status == "Diterima")).ToList();
                model.LokasiMenunggu = model.LokasiStatLabels.Select(u => data.Count(x => x.Lokasi == u && x.Status == "Menunggu")).ToList();
                model.LokasiDitolak = model.LokasiStatLabels.Select(u => data.Count(x => x.Lokasi == u && x.Status == "Ditolak")).ToList();
                ViewBag.SebaranTitle = $"Rekap Sebaran – {activeRegion}";
                ViewBag.SubTitle = "Rekap Per Fungsi / Unit Kerja";
            }

            ViewBag.TotalSemua = model.StatusDiproses + model.StatusDiterima + model.StatusDitolak;
            ViewBag.WeeklyTooltip = last7Days.Select(d => d.ToString("dddd, d MMMM yyyy", idCulture)).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            var adminRole = HttpContext.Session.GetString("AdminRole");
            var adminRegion = HttpContext.Session.GetString("AdminRegion");

            var query = _context.Notifications.AsNoTracking().AsQueryable();

            if (adminRole == "SuperAdmin")
            {
                query = query.Where(n => n.Type == "new");
            }
            else
            {
                if (!string.IsNullOrEmpty(adminRegion))
                {
                    var region = adminRegion.Trim().ToLower();
                    query = query.Where(n => n.Lokasi != null && n.Lokasi.ToLower().Trim() == region);
                }
            }

            var notifs = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new {
                    id = n.Id,
                    nama = n.Nama,
                    lokasi = n.Lokasi,
                    type = n.Type,
                    isRead = n.IsRead,
                    rawDate = n.CreatedAt
                })
                .ToListAsync();

            return Json(notifs);
        }

        [HttpPost]
        public async Task<JsonResult> MarkAsRead(int id)
        {
            var notif = await _context.Notifications.FindAsync(id);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailMahasiswa(int id)
        {
            var mhs = await _context.PendaftaranMagang.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (mhs == null) return NotFound(new { message = "Data tidak ditemukan" });

            return Json(new {
                id = mhs.Id,
                fotoProfil = GetFilePath(mhs.FotoProfil, "foto"),
                namaLengkap = mhs.NamaLengkap,
                emailPribadi = mhs.EmailPribadi,
                tempatLahir = mhs.TempatLahir,
                tanggalLahir = mhs.TanggalLahir.ToString("dd MMMM yyyy"),
                noHp = mhs.NoHp,
                instagram = mhs.Instagram,
                namaPerguruanTinggi = mhs.NamaPerguruanTinggi,
                fakultas = mhs.Fakultas,
                jurusan = mhs.Jurusan,
                nim = mhs.NIM,
                company = mhs.Company ?? "Pertamina Patra Niaga",
                region = mhs.Region,
                lokasi = mhs.Lokasi,
                rekomendasiPegawai = mhs.RekomendasiPegawai ?? "-",
                mulaiMagang = mhs.MulaiMagang.ToString("dd MMMM yyyy"),
                selesaiMagang = mhs.SelesaiMagang.ToString("dd MMMM yyyy"),
                fileCv = GetFilePath(mhs.FileCv, "cv"),
                fileSuratPengantar = GetFilePath(mhs.FileSuratPengantar, "surat"),
                fileProposal = GetFilePath(mhs.FileProposal, "proposal"),
                status = mhs.Status,
                tanggalDaftar = mhs.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMahasiswaByUnit(string unit, string status, string? region)
        {
            if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(status)) return Json(new List<object>());
            var adminRole = HttpContext.Session.GetString("AdminRole") ?? "SuperAdmin";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion") ?? "All";
            
            // Mengikuti logika yang sama dengan Index
            string activeRegion;
            if (!string.IsNullOrEmpty(region)) activeRegion = region;
            else if (adminRole == "SuperAdmin") activeRegion = "All";
            else activeRegion = sessionRegion;

            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin") query = query.Where(x => x.Region == sessionRegion);
            else if (activeRegion != "All" && activeRegion != "Semua Region") query = query.Where(x => x.Region == activeRegion);

            string u = unit.Trim().ToLower();
            string s = status.Trim().ToLower();

            if (activeRegion == "All" || activeRegion == "Semua Region")
                query = query.Where(x => x.Region != null && x.Region.Trim().ToLower() == u && x.Status.Trim().ToLower() == s);
            else
                query = query.Where(x => x.Lokasi != null && x.Lokasi.Trim().ToLower() == u && x.Status.Trim().ToLower() == s);

            var rawData = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return Json(MapToResult(rawData));
        }

        [HttpGet]
        public async Task<IActionResult> GetMahasiswaByKampus(string kampus, string? region)
        {
            if (string.IsNullOrEmpty(kampus)) return Json(new List<object>());
            var adminRole = HttpContext.Session.GetString("AdminRole") ?? "SuperAdmin";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion") ?? "All";
            
            string activeRegion;
            if (!string.IsNullOrEmpty(region)) activeRegion = region;
            else if (adminRole == "SuperAdmin") activeRegion = "All";
            else activeRegion = sessionRegion;

            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin") query = query.Where(x => x.Region == sessionRegion);
            else if (activeRegion != "All" && activeRegion != "Semua Region") query = query.Where(x => x.Region == activeRegion);

            string k = kampus.Trim().ToLower();
            var rawData = await query.Where(x => x.NamaPerguruanTinggi != null && x.NamaPerguruanTinggi.Trim().ToLower() == k)
                                     .OrderByDescending(x => x.CreatedAt).ToListAsync();
            return Json(MapToResult(rawData));
        }

        private List<object> MapToResult(List<Magang> rawData)
        {
            return rawData.Select(x => (object)new {
                id = x.Id,
                fotoProfil = GetFilePath(x.FotoProfil, "foto"),
                namaLengkap = x.NamaLengkap,
                emailPribadi = x.EmailPribadi,
                nim = x.NIM,
                namaPerguruanTinggi = x.NamaPerguruanTinggi,
                lokasi = x.Lokasi,
                status = x.Status,
                tanggalDaftar = x.CreatedAt.ToString("dd/MM/yyyy")
            }).ToList();
        }

        private string? GetFilePath(string? fileName, string folder)
        {
            if (string.IsNullOrEmpty(fileName)) 
                return folder == "foto" ? "/images/default-avatar.png" : null;

            string fileNameOnly = System.IO.Path.GetFileName(fileName);
            return $"/uploads/{folder}/{fileNameOnly}";
        }
    }
}