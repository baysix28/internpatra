
// ===============================
// DATA DESKRIPSI POSISI (KPI)
// ===============================
const deskripsiPosisi = {
    "Akuntansi / Ekonomi & Bisnis": "Analisis keuangan, akuntansi manajemen, dan studi kelayakan ekonomi proyek.",
    "Elektro (Arus Kuat)": "Desain dan pemeliharaan sistem distribusi listrik, pembangkit, dan jaringan tegangan tinggi.",
    "Elektro (Arus Lemah)": "Sistem kontrol, instrumentasi, otomasi, dan jaringan komunikasi industri.",
    "Emergency & Insurance": "Manajemen risiko, asuransi, dan penanganan keadaan darurat operasional.",
    "Health": "Kesehatan kerja, pemeriksaan medis, dan program kesehatan karyawan.",
    "Hukum": "Penelaahan kontrak, compliance regulasi, dan pendampingan hukum operasional.",
    "Ilmu Komunikasi / FISIP / Administrasi Publik": "Komunikasi korporat, public relations, dan administrasi publik.",
    "Internal Audit": "Audit operasional dan finansial untuk memastikan kepatuhan terhadap regulasi perusahaan.",
    "Kelautan / Perkapalan": "Operasi dan pemeliharaan fasilitas pelabuhan serta transportasi maritim.",
    "Kimia Murni / MIPA": "Analisis laboratorium, kontrol kualitas bahan baku, dan penelitian kimia terapan.",
    "Konversi Energi / Migas / Kimia Air Bersih / Blending / Loading": "Optimasi proses produksi, blending produk, dan operasi loading terminal.",
    "Logistik / Pergudangan / Procurement": "Manajemen rantai pasok, pergudangan, dan pengadaan barang/jasa.",
    "Manajemen / SDM / Psikologi": "Pengembangan SDM, rekrutmen, dan program pengembangan organisasi.",
    "Metalurgi / Material / Dirgantara": "Pengujian material, analisis kegagalan, dan seleksi material teknik.",
    "Safety (K3) / SMK3": "Implementasi sistem manajemen keselamatan kerja, inspeksi peralatan, dan prosedur darurat.",
    "Teknik Fisika": "Instrumentasi, kontrol proses, dan pemantauan kondisi peralatan.",
    "Teknik Industri": "Optimasi proses, studi kelayakan, dan perancangan sistem kerja.",
    "Teknik Informatika": "Maintenance sistem informasi, pengembangan aplikasi internal, dan dukungan infrastruktur IT.",
    "Teknik Kimia": "Optimasi proses pengolahan minyak mentah dan efisiensi energi produksi.",
    "Teknik Lingkungan": "Pengelolaan limbah, pemantauan kualitas lingkungan, dan program keberlanjutan.",
    "Teknik Mesin": "Perawatan dan perbaikan peralatan mekanik serta improvement reliability.",
    "Teknik Mesin (Rotating)": "Pemeliharaan dan analisis performa mesin rotasi seperti pompa, kompresor, dan turbin.",
    "Teknik Sipil": "Perawatan infrastruktur dan manajemen proyek fasilitas."
};

// ===============================
// DESKRIPSI PPN BERDASARKAN NAMA
// ===============================
function generateDeskripsiPPN(nama) {
    if (nama.includes("Integrated Terminal") || nama.startsWith("IT "))
        return "Terminal distribusi energi terintegrasi yang mengelola penyimpanan dan penyaluran BBM/LPG.";
    else if (nama.includes("Fuel Terminal") || nama.startsWith("FT "))
        return "Fasilitas penyimpanan dan distribusi BBM untuk wilayah operasional.";
    else if (nama.includes("DPPU") || nama.includes("AFT") || nama.includes("Aviation FT"))
        return "Unit distribusi avtur untuk mendukung operasional penerbangan.";
    else if (nama.includes("Retail") || nama.includes("Sales Area") || nama.includes("SAM") || nama.includes("SA "))
        return "Unit pengelolaan dan pengembangan jaringan SPBU serta penjualan energi ritel.";
    else if (nama.includes("Finance"))
        return "Unit pengelolaan keuangan, budgeting, dan pelaporan regional.";
    else if (nama.includes("HC") || nama.includes("Human Capital"))
        return "Unit pengelolaan sumber daya manusia dan pengembangan karyawan.";
    else if (nama.includes("HSSE"))
        return "Unit implementasi Health, Safety, Security & Environment.";
    else if (nama.includes("Legal Counsel"))
        return "Unit penanganan aspek hukum dan kepatuhan regulasi.";
    else if (nama.includes("Medical"))
        return "Unit pelayanan kesehatan kerja dan dukungan medis operasional.";
    else if (nama.includes("Procurement"))
        return "Unit pengadaan barang dan jasa untuk operasional regional.";
    else if (nama.includes("Supply") || nama.includes("S&D"))
        return "Unit pengelolaan rantai pasok dan distribusi energi.";
    else if (nama.includes("Comm") || nama.includes("CSR") || nama.includes("Communication"))
        return "Unit komunikasi korporat dan pelaksanaan program CSR.";
    else if (nama.includes("Project Dev") || nama.includes("Reliability"))
        return "Unit pengembangan proyek dan peningkatan keandalan aset.";
    else if (nama.includes("SSC ICT") || nama.includes("SCC ICT"))
        return "Unit dukungan sistem informasi dan infrastruktur teknologi.";
    else if (nama.includes("Marine"))
        return "Unit pengelolaan operasional transportasi laut.";
    else if (nama.includes("Asset Operation"))
        return "Unit pengelolaan aset dan pengawasan fasilitas distribusi energi.";
    else if (nama.includes("Corporate Sales"))
        return "Unit penjualan korporat untuk pelanggan industri dan komersial.";
    else if (nama.includes("Corp Operation"))
        return "Unit koordinasi operasional dan layanan korporat.";

    return "Unit operasional pendukung distribusi dan pengelolaan energi regional.";
}

// ===============================
// Script Tooltip Select2
// ===============================
$(document).ready(function() {
    if ($('#lokasi').length) {  // ✅ cek dulu ada atau tidak
        $('#lokasi').select2({
            placeholder: "Pilih Lokasi",
            width: '100%'
        });
    }


    const tooltipHover = document.getElementById('tooltipHover');
    const tTitle = document.getElementById('tooltipHoverTitle');
    const tDesc = document.getElementById('tooltipHoverDesc');

    // MENDETEKSI HOVER PADA LIST (SESUAI ARAHAN KURSOR)
    $(document).on('mouseenter', '.select2-results__option', function() {
        const text = $(this).text().trim();
        let deskripsi = null;

        // 1️⃣ CEK KPI
        if (deskripsiPosisi[text]) {
            deskripsi = deskripsiPosisi[text];
        }

        // 2️⃣ CEK PPN BERDASARKAN REGIONAL
        const selectedRegion = $('#region').val();
        // Pastikan variabel dataPPN sudah didefinisikan di tempat lain atau secara global
        if (typeof dataPPN !== 'undefined' && selectedRegion && dataPPN[selectedRegion]) {
            if (dataPPN[selectedRegion].includes(text)) {
                deskripsi = generateDeskripsiPPN(text);
            }
        }

        // 3️⃣ JIKA ADA DESKRIPSI → TAMPILKAN TOOLTIP
        if (deskripsi) {
            tTitle.innerText = text;
            tDesc.innerText = deskripsi;

            const rect = this.getBoundingClientRect();

            // Menggunakan window.scrollY agar posisi tetap akurat saat page di-scroll
            tooltipHover.style.top = (rect.top + window.scrollY) + 'px';
            tooltipHover.style.left = (rect.right + 20) + 'px';
            tooltipHover.classList.add('show');
        }
    });

    // SEMBUNYIKAN SAAT KELUAR DARI BARIS TERSEBUT
    $(document).on('mouseleave', '.select2-results__option', function() {
        if (tooltipHover) tooltipHover.classList.remove('show');
    });

    // SEMBUNYIKAN JIKA DROPDOWN TERTUTUP
    $('#lokasi').on('select2:closing', function() {
        if (tooltipHover) tooltipHover.classList.remove('show');
    });

    $('#lokasi').on('select2:select', function (e) {
        if (typeof saveDraft === "function") saveDraft();
        if (typeof cekDataLengkap === "function") cekDataLengkap();
    });
});