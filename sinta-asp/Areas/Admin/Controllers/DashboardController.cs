using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Areas.Admin.Models;
using sinta_asp.Models;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Validasi Session & Ambil Data Admin
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            // Mengambil admin berdasarkan Nama dari Session (yang sudah diupdate di Settings)
            var adminInfo = await _context.Admins
                .FirstOrDefaultAsync(a => a.Nama == adminNama);

            if (adminInfo == null || string.IsNullOrEmpty(adminInfo.RegionManaged))
                return Unauthorized("Akses ditolak: Admin tidak terhubung ke region manapun.");

            var model = new DashboardModel
            {
                AdminName = adminNama,
                LoginTime = DateTime.Now
            };

            // 2. Query Utama berdasarkan RegionManaged
            var query = _context.PendaftaranMagang
                .Where(x => x.Region == adminInfo.RegionManaged);

            // 3. Ringkasan Data Utama
            model.TotalInternAktif = await query.CountAsync();
            model.StatusDiproses = await query.CountAsync(x => x.Status == "Menunggu");
            model.StatusDiterima = await query.CountAsync(x => x.Status == "Diterima");
            model.StatusDitolak = await query.CountAsync(x => x.Status == "Ditolak");

            // 4. DATA TREN (Line Chart)
            int currentYear = DateTime.Now.Year;

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                model.WeeklyLabels.Add(date.ToString("dd MMM"));
                model.WeeklyCounts.Add(await query.CountAsync(x => x.CreatedAt.Date == date));
            }

            string[] namaBulan = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };
            for (int m = 1; m <= 12; m++)
            {
                model.MonthlyLabels.Add(namaBulan[m - 1]);
                model.MonthlyCounts.Add(await query.CountAsync(x => x.CreatedAt.Month == m && x.CreatedAt.Year == currentYear));
            }

            for (int y = currentYear - 2; y <= currentYear; y++)
            {
                model.YearlyLabels.Add(y.ToString());
                model.YearlyCounts.Add(await query.CountAsync(x => x.CreatedAt.Year == y));
            }

            // 5. Sebaran Data (Berdasarkan Unit per Region)
            var dataPPN = new Dictionary<string, List<string>>
            {
                { "Regional Jatimbalinus", new List<string> { "Asset Operation MOR V","Bitumen Plant Gresik","C&T IA Jatimbalinus","Comm, Rel, & CSR MOR V","Corporate Operation & Service Region V","Corporate Sales Region V","DPPU BIL","DPPU Eltari Group","DPPU Iswahyudi","DPPU Juanda","DPPU Ngurah Rai","Finance MOR V","Fuel Terminal Atapupu","Fuel Terminal Badas","Fuel Terminal Bima","Fuel Terminal Camplong","Fuel Terminal Ende","Fuel Terminal Kalabahi","Fuel Terminal Madiun","Fuel Terminal Malang","Fuel Terminal Maumere","Fuel Terminal Reo","Fuel Terminal Sanggaran","Fuel Terminal Tenau","Fuel Terminal Tuban","Fuel Terminal Waingapu","HC Jatimbalinus","HSSE Region V","Integrated Terminal Ampenan","Integrated Terminal Manggis","Integrated Terminal Surabaya","Integrated Terminal T. Wangi","Legal Counsel Regional Jatimbalinus","Marine Region V","Medical Jatimbalinus","Procurement MOR V","Rel & Project Dev Region V","Retail Bali","Retail Kediri","Retail Malang","Retail NTB","Retail NTT","Retail Sales Region V","Retail Surabaya","S&D Region V","SSC ICT VI Jatimbalinus" } },
                { "Regional Jawa Bagian Barat", new List<string> { "Asset Operation JBB","Corp. Opt & Serv JBB","Corporate Sales JBB","DPPU Halim PK Group","DPPU Husein Sastranegara","DPPU Kertajati","Finance JBB","Fuel Terminal Bandung Group","Fuel Terminal Cikampek","Fuel Terminal Tasikmalaya","Fuel Terminal Tg Gerem","HSSE JBB","Human Capital","Integrated Terminal Balongan","Integrated Terminal Jakarta","Legal Counsel JBB","Medical JBB","MWH & LPG Cylinder","Procurement JBB","Reliability & Project Dev JBB","SA Retail Bandung","SA Retail Cirebon","SA Retail Karawang","SA Retail Sukabumi","SAM Retail Banten","SAM Retail Jabode","SHAFTHI","SHIPS","SCC ICT JBB","Supply & Distribution JBB","Unit Comm, Rel & CSR JBB" } },
                { "Regional Jawa Bagian Tengah", new List<string> { "AFT Adi Sumarmo","AFT Adi Sucipto","AFT Ahmad Yani","AFT YIA","Fuel Terminal Boyolali","Fuel Terminal Lomanis","Fuel Terminal Maos","Fuel Terminal Rewulu","Fuel Terminal Tegal","Integrated Terminal Cilacap","Integrated Terminal Semarang","Kantor Branch Marketing DIY & Surakarta","Kantor Unit - Asset Operation JBT","Kantor Unit - Comm, Rel & CSR JBT","Kantor Unit - Corp Operation & Serv JBT","Kantor Unit - Corporate Sales JBT","Kantor Unit - Finance JBT","Kantor Unit - HC JBT","Kantor Unit - HSSE JBT","Kantor Unit - Internal Audit","Kantor Unit - Legal Counsel JBT","Kantor Unit - Medical JBT","Kantor Unit - Operational Risk JBT","Kantor Unit - Procurement JBT","Kantor Unit - Rel & Project Dev JBT","Kantor Unit - Retail Sales JBT","Kantor Unit - SSC ICT V JBT","Kantor Unit - Supply & Distribution JBT" } },
                { "Regional Kalimantan", new List<string> { "DPPU APT Pranoto","DPPU H. Asan","DPPU Iskandar","DPPU Juwata","DPPU Kalimaru","DPPU Sepinggan","DPPU Supadio","DPPU Syamsudin Noor","DPPU Tjilik Riwut","Fuel Terminal Pulang Pisau","Fuel Terminal Kotabaru","Fuel Terminal Pangkalan Bun","Fuel Terminal Samarinda","Fuel Terminal Sampit","Fuel Terminal Sintang","Fuel Terminal Tarakan","Integrated Terminal Balikpapan","Integrated Terminal Banjarmasin","Integrated Terminal Pontianak","Kantor Patra Niaga Region Kalimantan","SAM Retail Kalbar","SAM Retail Kalselteng","SAM Retail Kaltimut" } },
                { "Regional Maluku Papua", new List<string> { "Aviation FT Babullah","Aviation FT Deo","Aviation FT Depati Mopah","Aviation FT Depati Rendani","Aviation FT Dumatubun","Aviation FT Frans Kaisiepo","Aviation FT Mathilda","Aviation FT Mozes Kilangin","Aviation FT Paniai","Aviation FT Pattimura","Aviation FT Sentani","Aviation FT Utarom","FT Biak","FT Bula","FT Dobo","FT Fak-Fak","FT Kaimana","FT Labuha","FT Manokwari","FT Masohi","FT Merauke","FT Nabire","FT Namlea","FT Sanana","FT Saumlaki","FT Serui","FT Sorong","FT Ternate","FT Tobelo","FT Tual","IT Jayapura","IT Wayame","Kantor Region - Asset Operation Papua-Maluku","Kantor Region - Comm, Rel & CSR Papua-Maluku","Kantor Region - Corp Operation & Serv Papua-Maluku","Kantor Region - Corporate Sales Papua-Maluku","Kantor Region - Finance Papua-Maluku","Kantor Region - HC Papua-Maluku","Kantor Region - HSSE Papua-Maluku","Kantor Region - Legal Counsel Papua-Maluku","Kantor Region - Medical Papua-Maluku","Kantor Region - Procurement Papua-Maluku","Kantor Region - Rel & Project Dev Papua-Maluku","Kantor Region - Retail Sales Papua-Maluku","Kantor Region - Supply & Dist Papua-Maluku","Sales Area Ambon" } },
                { "Regional Sumbagut", new List<string> { "Asset Operation Region Sumbagut","Branch Marketing Aceh","Branch Marketing Kepulauan Riau","Branch Marketing Sibolga","Branch Marketing Sumbar","Communication & CSR Region Sumbagut","Corp Operation & Serv Region Sumbagut","Corporate Sales Region Sumbagut","DPPU Hang Nadim Group","DPPU Kualanamu Group","DPPU Minangkabau","DPPU SIM","DPPU SSK II","Finance Region Sumbagut","Fuel Terminal Batam","Fuel Terminal Gunung Sitoli","Fuel Terminal Kijang Group","Fuel Terminal Kisaran","Fuel Terminal Krueng Raya","Fuel Terminal Medan Group","Fuel Terminal Meulaboh","Fuel Terminal Natuna Group","Fuel Terminal Pematang Siantar","Fuel Terminal Sabang","Fuel Terminal Sei Siak","Fuel Terminal Sibolga","Fuel Terminal Simeulue","Fuel Terminal Tembilahan","HC Region Sumbagut","HSSE Region Sumbagut","IA Region I","Integrated Terminal Dumai","Integrated Terminal Lhokseumawe","Integrated Terminal Tanjung Uban","Integrated Terminal Teluk Kabung","Legal Counsel Region Sumbagut","Medical Region Sumbagut","Procurement Region Sumbagut","Rel & Project Dev Region Sumbagut","Retail Sales Region Sumbagut","SSC ICT I Region Sumbagut","Supply & Distribution Region Sumbagut" } }
            };

            List<string> currentUnitList = new List<string>();

            if (adminNama.Contains("RU VI"))
            {
                currentUnitList = new List<string> { "Teknik Informatika", "Teknik Kimia", "Teknik Mesin", "Teknik Elektro", "Akuntansi", "Manajemen" }; 
            }
            else if (dataPPN.ContainsKey(adminInfo.RegionManaged))
            {
                currentUnitList = dataPPN[adminInfo.RegionManaged];
            }

            var dbStats = await query.Select(x => new { x.Lokasi, x.Jurusan, x.Status }).ToListAsync();
            
            foreach (var unitName in currentUnitList)
            {
                model.LokasiStatLabels.Add(unitName);
                if (adminNama.Contains("RU VI"))
                {
                    model.LokasiDiterima.Add(dbStats.Count(x => x.Jurusan == unitName && x.Status == "Diterima"));
                    model.LokasiMenunggu.Add(dbStats.Count(x => x.Jurusan == unitName && x.Status == "Menunggu"));
                    model.LokasiDitolak.Add(dbStats.Count(x => x.Jurusan == unitName && x.Status == "Ditolak"));
                }
                else
                {
                    model.LokasiDiterima.Add(dbStats.Count(x => x.Lokasi == unitName && x.Status == "Diterima"));
                    model.LokasiMenunggu.Add(dbStats.Count(x => x.Lokasi == unitName && x.Status == "Menunggu"));
                    model.LokasiDitolak.Add(dbStats.Count(x => x.Lokasi == unitName && x.Status == "Ditolak"));
                }
            }

            // 6. Top 10 Kampus
            var kampusData = await query
                .GroupBy(m => m.NamaPerguruanTinggi)
                .Select(g => new { Nama = g.Key ?? "Tidak Diketahui", Jumlah = g.Count() })
                .OrderByDescending(x => x.Jumlah)
                .Take(10)
                .ToListAsync();

            model.KampusLabels = kampusData.Select(x => x.Nama).ToList();
            model.KampusCounts = kampusData.Select(x => x.Jumlah).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetMahasiswaByKampus(string kampus, string unit, string status)
        {
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama)) return Unauthorized();

            var adminInfo = await _context.Admins.FirstOrDefaultAsync(a => a.Nama == adminNama);
            if (adminInfo == null) return Unauthorized();

            var query = _context.PendaftaranMagang.Where(x => x.Region == adminInfo.RegionManaged);

            if (!string.IsNullOrEmpty(kampus))
            {
                query = query.Where(x => x.NamaPerguruanTinggi == kampus);
            }
            else if (!string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(status))
            {
                if (adminNama.Contains("RU VI"))
                {
                    query = query.Where(x => x.Jurusan == unit && x.Status == status);
                }
                else
                {
                    query = query.Where(x => x.Lokasi == unit && x.Status == status);
                }
            }

            var data = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();

            var result = data.Select(x => new {
                id = x.Id,
                nama = x.NamaLengkap,
                email = x.EmailPribadi,
                hp = x.NoHp,
                ig = x.Instagram,
                univ = x.NamaPerguruanTinggi,
                nim = x.NIM,
                fakultas = x.Fakultas,
                jurusan = x.Jurusan,
                company = x.Company,
                lokasi = x.Lokasi,
                rekomendasi = x.RekomendasiPegawai,
                tempatLahir = x.TempatLahir,
                tanggalLahir = x.TanggalLahir.ToString("dd MMMM yyyy"),
                tglMulai = x.MulaiMagang.ToString("dd MMM yyyy"),
                tglSelesai = x.SelesaiMagang.ToString("dd MMM yyyy"),
                fotoProfil = string.IsNullOrEmpty(x.FotoProfil) ? "/img/default-avatar.png" : "/uploads/foto/" + x.FotoProfil.Replace("uploads/foto/", "").TrimStart('/'),
                cvMahasiswa = string.IsNullOrEmpty(x.FileCv) ? "#" : "/uploads/cv/" + x.FileCv.Replace("uploads/cv/", "").TrimStart('/'),
                suratPengantar = string.IsNullOrEmpty(x.FileSuratPengantar) ? "#" : "/uploads/surat/" + x.FileSuratPengantar.Replace("uploads/surat/", "").TrimStart('/'),
                proposalMagang = string.IsNullOrEmpty(x.FileProposal) ? "#" : "/uploads/proposal/" + x.FileProposal.Replace("uploads/proposal/", "").TrimStart('/'),
                status = x.Status,
                createdAt = x.CreatedAt.ToString("dd MMM yyyy HH:mm")
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama)) return Unauthorized();

            var adminInfo = await _context.Admins.FirstOrDefaultAsync(a => a.Nama == adminNama);
            if (adminInfo == null) return Unauthorized();

            var limitTanggal = DateTime.Now.AddDays(-7);
            var hariIni = DateTime.Today;

            var dataNotif = await _context.PendaftaranMagang
                .Where(x => x.Region == adminInfo.RegionManaged && 
                           (x.CreatedAt >= limitTanggal || x.SelesaiMagang.Date == hariIni))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new {
                    id = x.Id,
                    nama = x.NamaLengkap,
                    jurusan = x.Jurusan,
                    lokasi = x.Lokasi,
                    type = (x.SelesaiMagang.Date == hariIni) ? "done" : "new",
                    rawDate = x.CreatedAt,
                    isRead = false // Default false agar Layout bisa menghitung badge
                })
                .ToListAsync();

            return Json(dataNotif);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            // Karena tidak ada tabel notifikasi fisik, kita kirim sukses saja 
            // Agar UI bisa mengupdate tampilan secara instan.
            return Json(new { success = true });
        }
    }
}