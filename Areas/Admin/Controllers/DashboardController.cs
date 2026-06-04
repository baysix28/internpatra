using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;
using sinta_asp.Areas.Admin.Models;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

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

        public async Task<IActionResult> Index(string? region, string? selectedUnit)
        {
            var adminRole     = HttpContext.Session.GetString("AdminRole")   ?? "AdminRegion";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion") ?? "";
            var adminName     = HttpContext.Session.GetString("AdminNama")   ?? "";

            var staticRegions = _masterDataUnit.Keys.ToList();
            var dbRegions = await _context.PendaftaranMagang
                .Select(x => x.Region)
                .Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x!)
                .ToListAsync();

            var allRegionsFromMaster = staticRegions
                .Union(dbRegions, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            bool isJawaTengah = string.Equals(sessionRegion, "Regional Jawa Bagian Tengah", StringComparison.OrdinalIgnoreCase);

            // BARU
            string activeRegion;
            if (adminRole == "SuperAdmin")
            {
                activeRegion = !string.IsNullOrEmpty(region) ? region : "All";
            }
            else if (isJawaTengah)
            {
                // Jawa Tengah bisa pilih All (Nasional) atau region manapun
                activeRegion = !string.IsNullOrEmpty(region) ? region : sessionRegion;
            }
            else
            {
                activeRegion = !string.IsNullOrEmpty(sessionRegion) ? sessionRegion : "All";
            }

            // ========== DATA MAGANG ==========
            // ========== DATA MAGANG ==========
            var magangQuery = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (adminRole == "SuperAdmin" || isJawaTengah)
            {
                if (activeRegion != "All" && activeRegion != "Semua Region")
                    magangQuery = magangQuery.Where(x => x.Region == activeRegion);
            }
            else
            {
                if (!string.IsNullOrEmpty(sessionRegion))
                    magangQuery = magangQuery.Where(x => x.Region == sessionRegion);
            }

            if (!string.IsNullOrEmpty(selectedUnit) && selectedUnit != "All")
            {
                if (activeRegion == "All" || activeRegion == "Semua Region")
                {
                    magangQuery = magangQuery.Where(x => x.Region == selectedUnit);
                }
                else
                {
                    magangQuery = magangQuery.Where(x => x.Lokasi == selectedUnit);
                }
            }

            var magangData = await magangQuery.ToListAsync();

            // ========== DATA PENELITIAN (dari tabel Pendaftaran) ==========
            var penelitianQuery = _context.Pendaftarans.AsNoTracking().AsQueryable();

            if (adminRole == "SuperAdmin" || isJawaTengah)
            {
                if (activeRegion != "All" && activeRegion != "Semua Region")
                    penelitianQuery = penelitianQuery.Where(x => x.Region == activeRegion);
            }
            else
            {
                if (!string.IsNullOrEmpty(sessionRegion))
                    penelitianQuery = penelitianQuery.Where(x => x.Region == sessionRegion);
            }
            if (!string.IsNullOrEmpty(selectedUnit) && selectedUnit != "All")
            {
                if (activeRegion == "All" || activeRegion == "Semua Region")
                {
                    penelitianQuery = penelitianQuery.Where(x => x.Region == selectedUnit);
                }
                else
                {
                    penelitianQuery = penelitianQuery.Where(x => x.LokasiPenelitian == selectedUnit);
                }
            }

            var penelitianData = await penelitianQuery.ToListAsync();

            var now         = DateTime.Now;
            var currentYear = now.Year;
            var idCulture   = new CultureInfo("id-ID");

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => now.Date.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var months = Enumerable.Range(1, 12).ToList();
            var years  = Enumerable.Range(2024, (currentYear - 2024) + 1).ToList();

            List<string> unitDropdown = new List<string>();
            if (activeRegion == "All" || activeRegion == "Semua Region")
            {
                unitDropdown = allRegionsFromMaster;
            }
            else
            {
                if (_masterDataUnit.ContainsKey(activeRegion))
                {
                    unitDropdown = _masterDataUnit[activeRegion];
                }
                else
                {
                    unitDropdown = magangData.Where(x => x.Region == activeRegion)
                                      .Select(x => x.Lokasi)
                                      .Distinct()
                                      .Where(l => !string.IsNullOrEmpty(l))
                                      .ToList()!;
                }
            }

            var model = new DashboardModel
            {
                AdminRole   = adminRole,
                AdminRegion = activeRegion,
                AdminName   = adminName,
                Regions     = allRegionsFromMaster,
                SelectedUnit = selectedUnit ?? "All",
                UnitDropdown = unitDropdown,

                // Data Magang
                StatusDiproses = magangData.Count(x => x.Status == "Menunggu" || x.Status == "Proses Review"),
                StatusDiterima = magangData.Count(x => x.Status == "Diterima"),
                StatusDitolak  = magangData.Count(x => x.Status == "Ditolak"),

                WeeklyLabels = last7Days.Select(d => d.ToString("dddd", idCulture)).ToList(),
                WeeklyCounts = last7Days.Select(d => magangData.Count(x => x.CreatedAt.Date == d)).ToList(),

                MonthlyLabels = months.Select(m => new DateTime(currentYear, m, 1).ToString("MMMM", idCulture)).ToList(),
                MonthlyCounts = months.Select(m => magangData.Count(x => x.CreatedAt.Month == m && x.CreatedAt.Year == currentYear)).ToList(),

                YearlyLabels = years.Select(y => y.ToString()).ToList(),
                YearlyCounts = years.Select(y => magangData.Count(x => x.CreatedAt.Year == y)).ToList(),

                KampusLabels = magangData
                    .GroupBy(x => x.NamaPerguruanTinggi)
                    .OrderByDescending(x => x.Count())
                    .Take(10)
                    .Select(x => x.Key ?? "N/A")
                    .ToList()!,
                KampusCounts = magangData
                    .GroupBy(x => x.NamaPerguruanTinggi)
                    .OrderByDescending(x => x.Count())
                    .Take(10)
                    .Select(x => x.Count())
                    .ToList(),

                DaftarMagang = magangData.OrderByDescending(x => x.CreatedAt).ToList(),

                // Data Penelitian (dari Pendaftaran)
                PenStatusDiproses = penelitianData.Count(x => x.Status == "Menunggu" || x.Status == "Proses Review"),
                PenStatusDiterima = penelitianData.Count(x => x.Status == "Diterima"),
                PenStatusDitolak  = penelitianData.Count(x => x.Status == "Ditolak"),

                PenWeeklyLabels = last7Days.Select(d => d.ToString("dddd", idCulture)).ToList(),
                PenWeeklyCounts = last7Days.Select(d => penelitianData.Count(x => x.CreatedAt.Date == d)).ToList(),

                PenMonthlyLabels = months.Select(m => new DateTime(currentYear, m, 1).ToString("MMMM", idCulture)).ToList(),
                PenMonthlyCounts = months.Select(m => penelitianData.Count(x => x.CreatedAt.Month == m && x.CreatedAt.Year == currentYear)).ToList(),

                PenYearlyLabels = years.Select(y => y.ToString()).ToList(),
                PenYearlyCounts = years.Select(y => penelitianData.Count(x => x.CreatedAt.Year == y)).ToList(),

                PenKampusLabels = penelitianData
                    .GroupBy(x => x.Universitas)
                    .OrderByDescending(x => x.Count())
                    .Take(10)
                    .Select(x => x.Key ?? "N/A")
                    .ToList()!,
                PenKampusCounts = penelitianData
                    .GroupBy(x => x.Universitas)
                    .OrderByDescending(x => x.Count())
                    .Take(10)
                    .Select(x => x.Count())
                    .ToList()
            };

            // Data Sebaran untuk Magang
            if (activeRegion == "All" || activeRegion == "Semua Region")
            {
                model.LokasiStatLabels = allRegionsFromMaster;
                model.LokasiDiterima   = model.LokasiStatLabels.Select(r => magangData.Count(x => x.Region == r && x.Status == "Diterima")).ToList();
                model.LokasiMenunggu   = model.LokasiStatLabels.Select(r => magangData.Count(x => x.Region == r && (x.Status == "Menunggu" || x.Status == "Proses Review"))).ToList();
                model.LokasiDitolak    = model.LokasiStatLabels.Select(r => magangData.Count(x => x.Region == r && x.Status == "Ditolak")).ToList();
                ViewBag.SebaranTitle   = "Rekap Sebaran Per Region";
                ViewBag.SubTitle       = "Nasional";
            }
            else
            {
                model.LokasiStatLabels = _masterDataUnit.ContainsKey(activeRegion)
                    ? _masterDataUnit[activeRegion]
                    : magangData.Where(x => x.Region == activeRegion)
                          .Select(x => x.Lokasi)
                          .Distinct()
                          .Where(l => !string.IsNullOrEmpty(l))
                          .ToList()!;

                model.LokasiDiterima = model.LokasiStatLabels.Select(u => magangData.Count(x => x.Lokasi == u && x.Status == "Diterima")).ToList();
                model.LokasiMenunggu = model.LokasiStatLabels.Select(u => magangData.Count(x => x.Lokasi == u && (x.Status == "Menunggu" || x.Status == "Proses Review"))).ToList();
                model.LokasiDitolak  = model.LokasiStatLabels.Select(u => magangData.Count(x => x.Lokasi == u && x.Status == "Ditolak")).ToList();
                ViewBag.SebaranTitle = $"Rekap Sebaran – {activeRegion}";
                ViewBag.SubTitle     = "Rekap Per Fungsi / Unit Kerja";
            }

            // Data Sebaran untuk Penelitian
            if (activeRegion == "All" || activeRegion == "Semua Region")
            {
                model.PenLokasiStatLabels = allRegionsFromMaster;
                model.PenLokasiDiterima   = model.PenLokasiStatLabels.Select(r => penelitianData.Count(x => x.Region == r && x.Status == "Diterima")).ToList();
                model.PenLokasiMenunggu   = model.PenLokasiStatLabels.Select(r => penelitianData.Count(x => x.Region == r && (x.Status == "Menunggu" || x.Status == "Proses Review"))).ToList();
                model.PenLokasiDitolak    = model.PenLokasiStatLabels.Select(r => penelitianData.Count(x => x.Region == r && x.Status == "Ditolak")).ToList();
            }
            else
            {
                model.PenLokasiStatLabels = _masterDataUnit.ContainsKey(activeRegion)
                    ? _masterDataUnit[activeRegion]
                    : penelitianData.Where(x => x.Region == activeRegion)
                          .Select(x => x.LokasiPenelitian)
                          .Distinct()
                          .Where(l => !string.IsNullOrEmpty(l))
                          .ToList()!;

                model.PenLokasiDiterima = model.PenLokasiStatLabels.Select(u => penelitianData.Count(x => x.LokasiPenelitian == u && x.Status == "Diterima")).ToList();
                model.PenLokasiMenunggu = model.PenLokasiStatLabels.Select(u => penelitianData.Count(x => x.LokasiPenelitian == u && (x.Status == "Menunggu" || x.Status == "Proses Review"))).ToList();
                model.PenLokasiDitolak  = model.PenLokasiStatLabels.Select(u => penelitianData.Count(x => x.LokasiPenelitian == u && x.Status == "Ditolak")).ToList();
            }

            ViewBag.TotalSemua    = model.StatusDiproses + model.StatusDiterima + model.StatusDitolak;
            ViewBag.PenTotalSemua = model.PenStatusDiproses + model.PenStatusDiterima + model.PenStatusDitolak;
            ViewBag.WeeklyTooltip = last7Days.Select(d => d.ToString("dddd, d MMMM yyyy", idCulture)).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            var adminIdStr  = HttpContext.Session.GetString("AdminId");
            var adminRole   = HttpContext.Session.GetString("AdminRole");
            var adminRegion = HttpContext.Session.GetString("AdminRegion");

            if (string.IsNullOrEmpty(adminIdStr)) return Json(new { success = false });
            int adminId = int.Parse(adminIdStr);

            var query = _context.AdminNotifications.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin" && !string.IsNullOrEmpty(adminRegion))
                query = query.Where(n => n.TargetRegion == adminRegion);

            var notifsRaw = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            var readNotifIds = await _context.AdminNotificationReads
                .Where(r => r.AdminId == adminId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            var result = notifsRaw.Select(n => new {
                id       = n.Id,
                title    = n.Title,
                message  = n.Message,
                type     = n.Type,
                isRead   = readNotifIds.Contains(n.Id),
                timeAgo  = CalculateTimeAgo(n.CreatedAt),
                magangId = n.MagangId,
                iconClass = n.Type == "Baru"     ? "fa-user-plus"    
                        : n.Type == "Diterima" ? "fa-check-circle"
                        : n.Type == "Ditolak"  ? "fa-times-circle"
                        : n.Type == "update"   ? "fa-pen-to-square"
                        : "fa-bell"
            });

            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> MarkAsRead(int id)
        {
            var adminIdStr = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminIdStr)) return Json(new { success = false });

            int adminId = int.Parse(adminIdStr);

            var sudahBaca = await _context.AdminNotificationReads
                .AnyAsync(r => r.NotificationId == id && r.AdminId == adminId);

            if (!sudahBaca)
            {
                _context.AdminNotificationReads.Add(new AdminNotificationRead
                {
                    NotificationId = id,
                    AdminId        = adminId,
                    ReadAt         = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailMahasiswa(int id)
        {
            var mhs = await _context.PendaftaranMagang.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (mhs == null) return NotFound(new { message = "Data tidak ditemukan" });

            return Json(new {
                id                  = mhs.Id,
                fotoProfil          = GetFilePath(mhs.FotoProfil, "Foto"),
                namaLengkap         = mhs.NamaLengkap,
                emailPribadi        = mhs.EmailPribadi,
                tempatLahir         = mhs.TempatLahir,
                tanggalLahir        = mhs.TanggalLahir.ToString("dd MMMM yyyy"),
                noHp                = mhs.NoHp,
                instagram           = mhs.Instagram,
                namaPerguruanTinggi = mhs.NamaPerguruanTinggi,
                fakultas            = mhs.Fakultas,
                jurusan             = mhs.Jurusan,
                nim                 = mhs.NIM,
                company             = mhs.Company ?? "Pertamina Patra Niaga",
                region              = mhs.Region,
                lokasi              = mhs.Lokasi,
                rekomendasiPegawai  = mhs.RekomendasiPegawai ?? "-",
                mulaiMagang         = mhs.MulaiMagang.ToString("dd MMMM yyyy"),
                selesaiMagang       = mhs.SelesaiMagang.ToString("dd MMMM yyyy"),
                fileCv              = GetFilePath(mhs.FileCv, "CV"),
                fileSuratPengantar  = GetFilePath(mhs.FileSuratPengantar, "Surat Magang"),
                fileProposal        = GetFilePath(mhs.FileProposal, "Proposal Magang"),
                status              = mhs.Status,
                tanggalDaftar       = mhs.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailPenelitian(int id)
        {
            var penelitian = await _context.Pendaftarans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (penelitian == null) return NotFound(new { message = "Data tidak ditemukan" });

            return Json(new {
                id               = penelitian.Id,
                nama             = penelitian.Nama,              // ✅ bukan NamaLengkap
                email            = penelitian.Email,
                noHp             = penelitian.NoHp,
                instagram        = penelitian.Instagram,
                tempatLahir      = penelitian.TempatLahir,
                tglLahir         = penelitian.TglLahir?.ToString("dd MMMM yyyy") ?? "-",  // ✅ nullable
                universitas      = penelitian.Universitas,       // ✅ bukan NamaPerguruanTinggi
                fakultas         = penelitian.Fakultas,
                jurusan          = penelitian.Jurusan,
                nim              = penelitian.Nim,               // ✅ bukan NIM
                judulPenelitian  = penelitian.JudulPenelitian,
                lokasiPenelitian = penelitian.LokasiPenelitian,
                region           = penelitian.Region,
                tglMulai         = penelitian.TglMulai?.ToString("dd MMMM yyyy") ?? "-",   // ✅ nullable
                tglSelesai       = penelitian.TglSelesai?.ToString("dd MMMM yyyy") ?? "-", // ✅ nullable
                pathFoto3x4      = GetFilePath(penelitian.PathFoto3x4, "foto"),
                pathCV           = GetFilePath(penelitian.PathCV, "CV Peneliti"),        // ✅ bukan FileCv
                pathSurat        = GetFilePath(penelitian.PathSurat, "Surat Penelitian"),  // ✅ bukan FileSuratPengantar
                pathProposal     = GetFilePath(penelitian.PathProposal, "Proposal Penelitian"),
                status           = penelitian.Status,
                tanggalDaftar    = penelitian.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMahasiswaByUnit(string unit, string status, string? region)
        {
            if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(status))
                return Json(new List<object>());

            var adminRole = HttpContext.Session.GetString("AdminRole") ?? "AdminRegion";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion") ?? "";

            string activeRegion = adminRole != "SuperAdmin"
                ? sessionRegion
                : (!string.IsNullOrEmpty(region) ? region : "All");

            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin")
                query = query.Where(x => x.Region == sessionRegion);
            else if (activeRegion != "All" && activeRegion != "Semua Region")
                query = query.Where(x => x.Region == activeRegion);

            string unitTrimmed = unit.Trim().ToLower();
            string statusTrimmed = status.Trim().ToLower();

            if (statusTrimmed == "menunggu")
            {
                query = query.Where(x => x.Status.ToLower() == "menunggu" || x.Status.ToLower() == "proses review");
            }
            else
            {
                query = query.Where(x => x.Status.ToLower() == statusTrimmed);
            }

            if (activeRegion == "All" || activeRegion == "Semua Region")
            {
                query = query.Where(x => x.Region != null && x.Region.ToLower() == unitTrimmed);
            }
            else
            {
                query = query.Where(x => x.Lokasi != null && x.Lokasi.ToLower() == unitTrimmed);
            }

            var rawData = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return Json(MapToResult(rawData));
        }

        [HttpGet]
        public async Task<IActionResult> GetPenelitianByUnit(string unit, string status, string? region)
        {
            if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(status))
                return Json(new List<object>());

            var adminRole     = HttpContext.Session.GetString("AdminRole")   ?? "AdminRegion";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion") ?? "";
            bool isJawaTengah = string.Equals(sessionRegion, "Regional Jawa Bagian Tengah", StringComparison.OrdinalIgnoreCase);

            string activeRegion = (adminRole == "SuperAdmin" || isJawaTengah)
                ? (!string.IsNullOrEmpty(region) ? region : "All")
                : sessionRegion;

            var query = _context.Pendaftarans.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin" && !isJawaTengah)
                query = query.Where(x => x.Region == sessionRegion);
            else if (activeRegion != "All" && activeRegion != "Semua Region")
                query = query.Where(x => x.Region == activeRegion);

            string unitTrimmed   = unit.Trim().ToLower();
            string statusTrimmed = status.Trim().ToLower();

            query = statusTrimmed == "menunggu"
                ? query.Where(x => x.Status.ToLower() == "menunggu" || x.Status.ToLower() == "proses review")
                : query.Where(x => x.Status.ToLower() == statusTrimmed);

            query = (activeRegion == "All" || activeRegion == "Semua Region")
                ? query.Where(x => x.Region           != null && x.Region.ToLower()           == unitTrimmed)
                : query.Where(x => x.LokasiPenelitian != null && x.LokasiPenelitian.ToLower() == unitTrimmed);

            var rawData = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return Json(MapPenelitianToResult(rawData));
        }

        [HttpGet]
        public async Task<IActionResult> GetMahasiswaByKampus(string kampus, string? region)
        {
            if (string.IsNullOrEmpty(kampus)) return Json(new List<object>());

            var adminRole     = HttpContext.Session.GetString("AdminRole")   ?? "AdminRegion";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion") ?? "";
            bool isJawaTengah = string.Equals(sessionRegion, "Regional Jawa Bagian Tengah", StringComparison.OrdinalIgnoreCase);

            string activeRegion = (adminRole == "SuperAdmin" || isJawaTengah)
                ? (!string.IsNullOrEmpty(region) ? region : "All")
                : sessionRegion;

            var query = _context.PendaftaranMagang.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin" && !isJawaTengah)
                query = query.Where(x => x.Region == sessionRegion);
            else if (activeRegion != "All" && activeRegion != "Semua Region")
                query = query.Where(x => x.Region == activeRegion);
            string k = kampus.Trim().ToLower();
            var rawData = await query
                .Where(x => x.NamaPerguruanTinggi != null && x.NamaPerguruanTinggi.Trim().ToLower() == k)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Json(MapToResult(rawData));
        }

        [HttpGet]
        public async Task<IActionResult> GetPenelitianByKampus(string kampus, string? region)
        {
            if (string.IsNullOrEmpty(kampus)) return Json(new List<object>());

            var adminRole     = HttpContext.Session.GetString("AdminRole")   ?? "AdminRegion";
            var sessionRegion = HttpContext.Session.GetString("AdminRegion") ?? "";
            bool isJawaTengah = string.Equals(sessionRegion, "Regional Jawa Bagian Tengah", StringComparison.OrdinalIgnoreCase);

            string activeRegion = (adminRole == "SuperAdmin" || isJawaTengah)
                ? (!string.IsNullOrEmpty(region) ? region : "All")
                : sessionRegion;

            var query = _context.Pendaftarans.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin" && !isJawaTengah)
                query = query.Where(x => x.Region == sessionRegion);
            else if (activeRegion != "All" && activeRegion != "Semua Region")
                query = query.Where(x => x.Region == activeRegion);

            string k = kampus.Trim().ToLower();
            var rawData = await query
                .Where(x => x.Universitas != null && x.Universitas.Trim().ToLower() == k)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Json(MapPenelitianToResult(rawData));
        }
        [HttpPost]
        public async Task<JsonResult> MarkAllAsRead()
        {
            var adminIdStr = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminIdStr)) return Json(new { success = false });

            int adminId = int.Parse(adminIdStr);
            var adminRole   = HttpContext.Session.GetString("AdminRole");
            var adminRegion = HttpContext.Session.GetString("AdminRegion");

            // Ambil semua notif yang relevan untuk admin ini
            var query = _context.AdminNotifications.AsNoTracking().AsQueryable();

            if (adminRole != "SuperAdmin" && !string.IsNullOrEmpty(adminRegion))
                query = query.Where(n => n.TargetRegion == adminRegion);

            var allNotifIds = await query.Select(n => n.Id).ToListAsync();

            // Cari yang belum dibaca
            var sudahBacaIds = await _context.AdminNotificationReads
                .Where(r => r.AdminId == adminId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            var belumBacaIds = allNotifIds.Except(sudahBacaIds).ToList();

            if (belumBacaIds.Any())
            {
                var newReads = belumBacaIds.Select(notifId => new AdminNotificationRead
                {
                    NotificationId = notifId,
                    AdminId        = adminId,
                    ReadAt         = DateTime.Now
                });

                _context.AdminNotificationReads.AddRange(newReads);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private List<object> MapToResult(List<Magang> rawData)
        {
            return rawData.Select(x => (object)new {
                id                  = x.Id,
                fotoProfil          = GetFilePath(x.FotoProfil, "foto"),
                namaLengkap         = x.NamaLengkap,
                emailPribadi        = x.EmailPribadi,
                nim                 = x.NIM,
                namaPerguruanTinggi = x.NamaPerguruanTinggi,
                lokasi              = x.Lokasi,
                status              = x.Status,
                tanggalDaftar       = x.CreatedAt.ToString("dd/MM/yyyy")
            }).ToList();
        }

        private List<object> MapPenelitianToResult(List<Pendaftaran> rawData)
        {
            return rawData.Select(x => (object)new
            {
                id                  = x.Id,
                fotoProfil          = GetFilePath(x.PathFoto3x4, "foto"), // ✅
                nama                = x.Nama,                              // ✅
                email               = x.Email,
                nim                 = x.Nim,                               // ✅
                namaPerguruanTinggi = x.Universitas,                       // ✅
                lokasi              = x.LokasiPenelitian,
                status              = x.Status,
                tanggalDaftar       = x.CreatedAt.ToString("dd/MM/yyyy")
            }).ToList();
        }

        private string? GetFilePath(string? fileName, string folder)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                var nama = User?.FindFirst("AdminNama")?.Value ?? "User";
                return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(nama)}&background=0D8ABC&color=fff&bold=true";
            }

            string fileNameOnly = System.IO.Path.GetFileName(fileName);
            string encodedFileName = Uri.EscapeDataString(fileNameOnly);

            return $"/uploads/{folder}/{encodedFileName}";
        }

        private string CalculateTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalMinutes < 1)  return "Baru saja";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} menit lalu";
            if (span.TotalHours < 24)   return $"{(int)span.TotalHours} jam lalu";
            if (span.TotalDays < 7)     return $"{(int)span.TotalDays} hari lalu";
            return dt.ToString("dd MMMM yyyy", new CultureInfo("id-ID"));
        }
    }
}