// ================= DATA COMPANY =================

const dataKPI = {
    "Refinery Unit VI Balongan": [
        "Akuntansi / Ekonomi & Bisnis",
        "Elektro (Arus Kuat)",
        "Elektro (Arus Lemah)",
        "Emergency & Insurance",
        "Health",
        "Hukum",
        "Ilmu Komunikasi / FISIP / Administrasi Publik",
        "Internal Audit",
        "Kelautan / Perkapalan",
        "Kimia Murni / MIPA",
        "Konversi Energi / Migas / Kimia Air Bersih / Blanding / Loading",
        "Logistik / Pergudangan / Procurement",
        "Manajemen / SDM / Psikologi",
        "Metalurgi / Material / Dirgantara",
        "Safety (K3) / SMK3",
        "Teknik Fisika",
        "Teknik Industri",
        "Teknik Informatika",
        "Teknik Kimia",
        "Teknik Lingkungan",
        "Teknik Mesin",
        "Teknik Mesin (Rotating)",
        "Teknik Sipil"
    ]
};

const dataPPN = {
    "Regional Jatimbalinus": [
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
        "S&D Region V","SSC ICT VI Jatimbalinus","XXX"
    ],
    "Regional Jawa Bagian Barat": [
        "Asset Operation JBB","Corp. Opt & Serv JBB","Corporate Sales JBB","DPPU Halim PK Group",
        "DPPU Husein Sastranegara","DPPU Kertajati","Finance JBB","Fuel Terminal Bandung Group",
        "Fuel Terminal Cikampek","Fuel Terminal Tasikmalaya","Fuel Terminal Tg Gerem","HSSE JBB",
        "Human Capital","Integrated Terminal Balongan","Integrated Terminal Jakarta","Legal Counsel JBB",
        "Medical JBB","MWH & LPG Cylinder","Procurement JBB","Reliability & Project Dev JBB",
        "SA Retail Bandung","SA Retail Cirebon","SA Retail Karawang","SA Retail Sukabumi",
        "SAM Retail Banten","SAM Retail Jabode","SHAFTHI","SHIPS","SCC ICT JBB",
        "Supply & Distribution JBB","Unit Comm, Rel & CSR JBB"
    ],
    "Regional Jawa Bagian Tengah": [
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
    ],
    "Regional Kalimantan": [
        "DPPU APT Pranoto","DPPU H. Asan","DPPU Iskandar","DPPU Juwata",
        "DPPU Kalimaru","DPPU Sepinggan","DPPU Supadio","DPPU Syamsudin Noor",
        "DPPU Tjilik Riwut","Fuel Terminal Pulang Pisau","Fuel Terminal Kotabaru",
        "Fuel Terminal Pangkalan Bun","Fuel Terminal Samarinda","Fuel Terminal Sampit",
        "Fuel Terminal Sintang","Fuel Terminal Tarakan","Integrated Terminal Balikpapan",
        "Integrated Terminal Banjarmasin","Integrated Terminal Pontianak",
        "Kantor Patra Niaga Region Kalimantan","SAM Retail Kalbar","SAM Retail Kalselteng",
        "SAM Retail Kaltimut"
    ],
    "Regional Maluku Papua": [
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
    ],
    "Regional Sumbagut": [
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
    ]
};


// ===== DROPDOWN LOGIC =====
document.getElementById('company')?.addEventListener('change', function () {
    const regionSelect = document.getElementById('region');
    const lokasiSelect = document.getElementById('lokasi');

    regionSelect.innerHTML = '<option value="">Pilih Region</option>';
    lokasiSelect.innerHTML = '<option value="">Pilih Lokasi</option>';

    let regions = [];

    if (this.value === 'KPI') {
        regions = Object.keys(dataKPI);
    } else if (this.value === 'PPN') {
        regions = Object.keys(dataPPN);
    }

    regions.forEach(region => {
        const option = document.createElement('option');
        option.value = region;
        option.textContent = region;
        regionSelect.appendChild(option);
    });
});

document.getElementById('region')?.addEventListener('change', function () {
    const lokasiSelect = document.getElementById('lokasi');
    lokasiSelect.innerHTML = '<option value="">Pilih Lokasi</option>';

    let lokasiList = [];
    const company = document.getElementById('company').value;

    if (company === 'KPI' && dataKPI[this.value]) {
        lokasiList = dataKPI[this.value];
    } else if (company === 'PPN' && dataPPN[this.value]) {
        lokasiList = dataPPN[this.value];
    }

    lokasiList.forEach(lokasi => {
        const option = document.createElement('option');
        option.value = lokasi;
        option.textContent = lokasi;
        lokasiSelect.appendChild(option);
    });
});
