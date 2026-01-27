using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Areas.Admin.Models;

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
            // 1. Validasi Session Admin
            var adminNama = HttpContext.Session.GetString("AdminNama");
            if (string.IsNullOrEmpty(adminNama))
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var model = new DashboardModel
            {
                AdminName = adminNama,
                LoginTime = DateTime.Now
            };

            var query = _context.PendaftaranMagang.AsQueryable();
            List<string> listUnitOrJurusan = new();

            // 2. Filter Regional & Inisialisasi List Label Berdasarkan Otoritas
            if (adminNama == "Admin MOR I")
            {
                listUnitOrJurusan = new List<string> { "Asset Operation Region Sumbagut", "Branch Marketing Aceh", "Branch Marketing Kepulauan Riau", "Branch Marketing Sibolga", "Branch Marketing Sumbar", "Communication & CSR Region Sumbagut", "Corp Operation & Serv Region Sumbagut", "Corporate Sales Region Sumbagut", "DPPU Hang Nadim Group", "DPPU Kualanamu Group", "DPPU Minangkabau", "DPPU SIM", "DPPU SSK II", "Finance Region Sumbagut", "Fuel Terminal Batam", "Fuel Terminal Gunung Sitoli", "Fuel Terminal Kijang Group", "Fuel Terminal Kisaran", "Fuel Terminal Krueng Raya", "Fuel Terminal Medan Group", "Fuel Terminal Meulaboh", "Fuel Terminal Natuna Group", "Fuel Terminal Pematang Siantar", "Fuel Terminal Sabang", "Fuel Terminal Sei Siak", "Fuel Terminal Sibolga", "Fuel Terminal Simeulue", "Fuel Terminal Tembilahan", "HC Region Sumbagut", "HSSE Region Sumbagut", "IA Region I", "Integrated Terminal Dumai", "Integrated Terminal Lhokseumawe", "Integrated Terminal Tanjung Uban", "Integrated Terminal Teluk Kabung", "Legal Counsel Region Sumbagut", "Medical Region Sumbagut", "Procurement Region Sumbagut", "Rel & Project Dev Region Sumbagut", "Retail Sales Region Sumbagut", "SSC ICT I Region Sumbagut", "Supply & Distribution Region Sumbagut" };
                query = query.Where(x => x.Region == "Regional Sumbagut");
            }
            else if (adminNama == "Admin MOR II")
            {
                listUnitOrJurusan = new List<string> { "AFT Depati Amir", "AFT Fatmawati Soekarno", "AFT Gatot Subroto", "AFT Silampari", "AFT SMB II", "AFT Sultan Thaha", "Branch Marketing Bengkulu", "Branch Marketing Jambi", "Branch Marketing Lampung", "Branch Marketing Sumsel", "DPPU Radin Inten II", "Fuel Terminal Baturaja", "Fuel Terminal Jambi", "Fuel Terminal Lahat", "Fuel Terminal Linggau", "Fuel Terminal Lubuk Linggau", "Fuel Terminal Panjang", "Fuel Terminal Pulau Baai", "Integrated Terminal Palembang", "Integrated Terminal Panjang", "Kantor Region - Asset Operation Sumbagsel", "Kantor Region - Finance Sumbagsel", "Kantor Region - HC Sumbagsel", "Kantor Region - HSSE Sumbagsel", "Kantor Region - Medical Sumbagsel", "Retail Sales Region Sumbagsel" };
                query = query.Where(x => x.Region == "Regional Sumbagsel");
            }
            else if (adminNama == "Admin MOR III")
            {
                listUnitOrJurusan = new List<string> { "Asset Operation JBB", "Corp. Opt & Serv JBB", "Corporate Sales JBB", "DPPU Halim PK Group", "DPPU Husein Sastranegara", "DPPU Kertajati", "Finance JBB", "Fuel Terminal Bandung Group", "Fuel Terminal Cikampek", "Fuel Terminal Tasikmalaya", "Fuel Terminal Tg Gerem", "HSSE JBB", "Human Capital", "Integrated Terminal Balongan", "Integrated Terminal Jakarta", "Legal Counsel JBB", "Medical JBB", "MWH & LPG Cylinder", "Procurement JBB", "Reliability & Project Dev JBB", "SA Retail Bandung", "SA Retail Cirebon", "SA Retail Karawang", "SA Retail Sukabumi", "SAM Retail Banten", "SAM Retail Jabode", "SHAFTHI", "SHIPS", "SCC ICT JBB", "Supply & Distribution JBB", "Unit Comm, Rel & CSR JBB" };
                query = query.Where(x => x.Region == "Regional Jawa Bagian Barat");
            }
            else if (adminNama == "Admin MOR IV")
            {
                listUnitOrJurusan = new List<string> { "AFT Adi Sumarmo", "AFT Adi Sucipto", "AFT Ahmad Yani", "AFT YIA", "Fuel Terminal Boyolali", "Fuel Terminal Lomanis", "Fuel Terminal Maos", "Fuel Terminal Rewulu", "Fuel Terminal Tegal", "Integrated Terminal Cilacap", "Integrated Terminal Semarang", "Kantor Branch Marketing DIY & Surakarta", "Kantor Unit - Asset Operation JBT", "Kantor Unit - Comm, Rel & CSR JBT", "Kantor Unit - Corp Operation & Serv JBT", "Kantor Unit - Corporate Sales JBT", "Kantor Unit - Finance JBT", "Kantor Unit - HC JBT", "Kantor Unit - HSSE JBT", "Kantor Unit - Internal Audit", "Kantor Unit - Legal Counsel JBT", "Kantor Unit - Medical JBT", "Kantor Unit - Operational Risk JBT", "Kantor Unit - Procurement JBT", "Kantor Unit - Rel & Project Dev JBT", "Kantor Unit - Retail Sales JBT", "Kantor Unit - SSC ICT V JBT", "Kantor Unit - Supply & Distribution JBT" };
                query = query.Where(x => x.Region == "Regional Jawa Bagian Tengah");
            }
            else if (adminNama == "Admin MOR V")
            {
                listUnitOrJurusan = new List<string> { "Asset Operation MOR V", "Bitumen Plant Gresik", "C&T IA Jatimbalinus", "Comm, Rel, & CSR MOR V", "Corporate Operation & Service Region V", "Corporate Sales Region V", "DPPU BIL", "DPPU Eltari Group", "DPPU Iswahyudi", "DPPU Juanda", "DPPU Ngurah Rai", "Finance MOR V", "Fuel Terminal Atapupu", "Fuel Terminal Badas", "Fuel Terminal Bima", "Fuel Terminal Camplong", "Fuel Terminal Ende", "Fuel Terminal Kalabahi", "Fuel Terminal Madiun", "Fuel Terminal Malang", "Fuel Terminal Maumere", "Fuel Terminal Reo", "Fuel Terminal Sanggaran", "Fuel Terminal Tenau", "Fuel Terminal Tuban", "Fuel Terminal Waingapu", "HC Jatimbalinus", "HSSE Region V", "Integrated Terminal Ampenan", "Integrated Terminal Manggis", "Integrated Terminal Surabaya", "Integrated Terminal T. Wangi", "Legal Counsel Regional Jatimbalinus", "Marine Region V", "Medical Jatimbalinus", "Procurement MOR V", "Rel & Project Dev Region V", "Retail Bali", "Retail Kediri", "Retail Malang", "Retail NTB", "Retail NTT", "Retail Sales Region V", "Retail Surabaya", "S&D Region V", "SSC ICT VI Jatimbalinus" };
                query = query.Where(x => x.Region == "Regional Jatimbalinus");
            }
            else if (adminNama == "Admin MOR VI")
            {
                listUnitOrJurusan = new List<string> { "DPPU APT Pranoto", "DPPU H. Asan", "DPPU Iskandar", "DPPU Juwata", "DPPU Kalimaru", "DPPU Sepinggan", "DPPU Supadio", "DPPU Syamsudin Noor", "DPPU Tjilik Riwut", "Fuel Terminal Pulang Pisau", "Fuel Terminal Kotabaru", "Fuel Terminal Pangkalan Bun", "Fuel Terminal Samarinda", "Fuel Terminal Sampit", "Fuel Terminal Sintang", "Fuel Terminal Tarakan", "Integrated Terminal Balikpapan", "Integrated Terminal Banjarmasin", "Integrated Terminal Pontianak", "Kantor Patra Niaga Region Kalimantan", "SAM Retail Kalbar", "SAM Retail Kalselteng", "SAM Retail Kaltimut" };
                query = query.Where(x => x.Region == "Regional Kalimantan");
            }
            else if (adminNama == "Admin MOR VIII")
            {
                listUnitOrJurusan = new List<string> { "Aviation FT Babullah", "Aviation FT Deo", "Aviation FT Depati Mopah", "Aviation FT Depati Rendani", "Aviation FT Dumatubun", "Aviation FT Frans Kaisiepo", "Aviation FT Mathilda", "Aviation FT Mozes Kilangin", "Aviation FT Paniai", "Aviation FT Pattimura", "Aviation FT Sentani", "Aviation FT Utarom", "FT Biak", "FT Bula", "FT Dobo", "FT Fak-Fak", "FT Kaimana", "FT Labuha", "FT Manokwari", "FT Masohi", "FT Merauke", "FT Nabire", "FT Namlea", "FT Sanana", "FT Saumlaki", "FT Serui", "FT Sorong", "FT Ternate", "FT Tobelo", "FT Tual", "IT Jayapura", "IT Wayame", "Kantor Region - Asset Operation Papua-Maluku", "Kantor Region - Comm, Rel & CSR Papua-Maluku", "Kantor Region - Corp Operation & Serv Papua-Maluku", "Kantor Region - Corporate Sales Papua-Maluku", "Kantor Region - Finance Papua-Maluku", "Kantor Region - HC Papua-Maluku", "Kantor Region - HSSE Papua-Maluku", "Kantor Region - Legal Counsel Papua-Maluku", "Kantor Region - Medical Papua-Maluku", "Kantor Region - Procurement Papua-Maluku", "Kantor Region - Rel & Project Dev Papua-Maluku", "Kantor Region - Retail Sales Papua-Maluku", "Kantor Region - Supply & Dist Papua-Maluku", "Sales Area Ambon" };
                query = query.Where(x => x.Region == "Regional Maluku Papua");
            }
            else if (adminNama == "Admin RU VI")
            {
                listUnitOrJurusan = new List<string> { "Akuntansi / Ekonomi & Bisnis", "Elektro (Arus Kuat)", "Elektro (Arus Lemah)", "Emergency & Insurance", "Health", "Hukum", "Ilmu Komunikasi / FISIP / Administrasi Publik", "Internal Audit", "Kelautan / Perkapalan", "Kimia Murni / MIPA", "Konversi Energi / Migas / Kimia Air Bersih / Blanding / Loading", "Logistik / Pergudangan / Procurement", "Manajemen / SDM / Psikologi", "Metalurgi / Material / Dirgantara", "Safety (K3) / SMK3", "Teknik Fisika", "Teknik Industri", "Teknik Informatika", "Teknik Kimia", "Teknik Lingkungan", "Teknik Mesin", "Teknik Mesin (Rotating)", "Teknik Sipil" };
                query = query.Where(x => x.Region == "Refinery Unit VI Balongan");
            }

            // 3. Ringkasan Data Utama (Counter)
            model.TotalInternAktif = await query.CountAsync();
            model.StatusDiproses = await query.CountAsync(x => x.Status == "Menunggu");
            model.StatusDiterima = await query.CountAsync(x => x.Status == "Diterima");
            model.StatusDitolak = await query.CountAsync(x => x.Status == "Ditolak");

            // 4. Tren Mingguan (7 Hari Terakhir)
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                model.WeeklyLabels.Add(date.ToString("dd MMM"));
                // Pastikan kolom 'CreatedAt' ada di entitas PendaftaranMagang Anda
                model.WeeklyCounts.Add(await query.CountAsync(x => x.CreatedAt.Date == date));
            }

            // 5. Tren Bulanan (6 Bulan Terakhir)
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.Now.AddMonths(-i);
                model.MonthlyLabels.Add(monthDate.ToString("MMM yyyy"));
                model.MonthlyCounts.Add(await query.CountAsync(x => x.CreatedAt.Month == monthDate.Month && x.CreatedAt.Year == monthDate.Year));
            }

            // 6. Sebaran Data (Chart Bar - Berdasarkan Unit/Jurusan)
            // Mengambil data ke memori untuk efisiensi filtering di loop
            var dbStats = await query.Select(x => new { x.Lokasi, x.Jurusan, x.Status }).ToListAsync();

            foreach (var label in listUnitOrJurusan)
            {
                model.LokasiStatLabels.Add(label);

                if (adminNama == "Admin RU VI")
                {
                    // Admin Kilang filter berdasarkan JURUSAN
                    model.LokasiDiterima.Add(dbStats.Count(x => x.Jurusan == label && x.Status == "Diterima"));
                    model.LokasiMenunggu.Add(dbStats.Count(x => x.Jurusan == label && x.Status == "Menunggu"));
                    model.LokasiDitolak.Add(dbStats.Count(x => x.Jurusan == label && x.Status == "Ditolak"));
                }
                else
                {
                    // Admin MOR filter berdasarkan LOKASI/UNIT
                    model.LokasiDiterima.Add(dbStats.Count(x => x.Lokasi == label && x.Status == "Diterima"));
                    model.LokasiMenunggu.Add(dbStats.Count(x => x.Lokasi == label && x.Status == "Menunggu"));
                    model.LokasiDitolak.Add(dbStats.Count(x => x.Lokasi == label && x.Status == "Ditolak"));
                }
            }

            return View(model);
        }
    }
}