function showStep(n) {
    document.querySelectorAll('.step-item').forEach(item => item.classList.remove('step-active'));
    document.getElementById('step-tab-' + n).classList.add('step-active');
    document.querySelectorAll('.step-content').forEach(content => content.classList.remove('active'));
    document.getElementById('content-' + n).classList.add('active');

    window.scrollTo(0,0);
    cekDataLengkap();
}

function cekDataLengkap() {
    for (let i = 1; i <= 4; i++) {
        const container = document.getElementById('content-' + i);
        const inputs = container.querySelectorAll('.wajib');
        let kosong = false;

        inputs.forEach(input => {
            if (input.type === 'file') {
                if (input.files.length === 0) kosong = true;
            } else {
                if (!input.value.trim()) kosong = true;
            }
        });

        // Khusus step 1 tambah cek foto
        if (i === 1 && !fotoTerisi) kosong = true;
        document.getElementById('dot-' + i).style.display = kosong ? 'flex' : 'none';
    }
}

// Listener Real-time
document.querySelectorAll('.wajib').forEach(input => {
    input.addEventListener('input', cekDataLengkap);
    input.addEventListener('change', cekDataLengkap);
});

function simpanSemua() {
    // 1. Jalankan pengecekan ulang untuk memastikan semua dot merah terupdate
    cekDataLengkap();

    let semuaLengkap = true;
    const errorFinal = document.getElementById('error-final');

    // 2. Cek apakah masih ada dot merah di stepper (1-4)
    for (let i = 1; i <= 4; i++) {
        const dot = document.getElementById('dot-' + i);
        if (dot && dot.style.display === 'flex') {
            semuaLengkap = false;
            break;
        }
    }

    // 3. Logika tampilan tanpa alert
    if (!semuaLengkap) {
        // Munculkan pesan error di halaman
        errorFinal.style.display = 'flex';
            
        // Scroll halus ke arah pesan error agar user melihatnya
        errorFinal.scrollIntoView({ behavior: 'smooth', block: 'center' });
            
        // Opsional: Hilangkan pesan secara otomatis setelah 5 detik
        setTimeout(() => {
            errorFinal.style.display = 'none';
        }, 5000);

        return; // Berhenti di sini, jangan lanjut ke modal
    }

    // 4. Jika sudah lengkap, sembunyikan error (jika ada) dan buka modal konfirmasi
    errorFinal.style.display = 'none';
    document.getElementById('modalKonfirmasi').classList.add('show');
}

$("#formMagang").submit(function (e) {
    e.preventDefault();

    var formData = new FormData(this);

    $.ajax({
        url: '@Url.Action("Store", "PendaftaranMagang")',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            if (res.success) {
                window.location.href = res.redirectUrl;
            } else {
                alert(res.message);
                console.log(res.errors);
            }
        },
        error: function () {
            alert("Server error");
        }
    });
});
    
function tutupModal() {
    document.getElementById('modalKonfirmasi').classList.remove('show');
}

function konfirmasiKirim() {
    console.log("FORM DIKIRIM");
    tutupModal();
    document.getElementById('formMagang').submit();
}

// Jalankan ini di halaman DataMagang.cshtml setiap kali input diisi
function updateSavedProgress() {
    const inputs = document.querySelectorAll('.profile-input');
    const filled = Array.from(inputs).filter(i => i.value.trim() !== "").length;
    const percent = Math.round((filled / inputs.length) * 100);
        
    // Simpan ke storage agar bisa dibaca halaman Dashboard
    localStorage.setItem('profile_completion', percent);
}

$(document).ready(function() {
    $('#no_hp').on('input', function() {
        // 1. Ambil nilai dan buang karakter non-angka
        let value = $(this).val().replace(/\D/g, '');
        $(this).val(value); // Update field hanya dengan angka

        const errorMsg = $('#hp-error');
            
        // 2. Validasi: Jika sudah mulai mengetik, cek apakah diawali 08
        if (value.length >= 2) {
            if (value.substring(0, 2) !== '08') {
                $(this).addClass('is-invalid');
                errorMsg.css('display', 'flex').text('Nomor HP harus diawali dengan 08');
            } else if (value.length < 10) {
                $(this).addClass('is-invalid');
                errorMsg.css('display', 'flex').text('Nomor HP minimal 10 digit');
            } else {
                $(this).removeClass('is-invalid');
                errorMsg.hide();
            }
        } else if (value.length > 0 && value[0] !== '0') {
            // Cek digit pertama harus 0
            $(this).addClass('is-invalid');
            errorMsg.css('display', 'flex').text('Nomor HP harus diawali dengan 08');
        } else {
            // Jika kosong atau baru 1 digit benar (angka 0)
            $(this).removeClass('is-invalid');
            errorMsg.hide();
        }
    });
});

// --- SISTEM DRAFT GOOGLE FORM STYLE ---
// 1. Fungsi untuk menyimpan semua input ke localStorage
function saveDraft() {
    const formData = {};
    const inputs = document.querySelectorAll('input, select, textarea');
        
    inputs.forEach(input => {
        if (input.id) {
            if (input.type === 'file') {
                // Kita tidak bisa simpan filenya, tapi kita simpan statusnya
                // agar sistem tahu user sudah pernah upload
                formData[input.id + "_status"] = input.files.length > 0 ? "uploaded" : "empty";
            } else {
                formData[input.id] = input.value;
            }
        }
    });

    localStorage.setItem('form_magang_draft', JSON.stringify(formData));
}

// 2. Fungsi untuk memuat data saat halaman dibuka kembali
function loadDraft() {
    const savedData = localStorage.getItem('form_magang_draft');
    if (!savedData) return;

    const formData = JSON.parse(savedData);

    // 1. Isi Company dulu
    if (formData['company']) {
        const comp = document.getElementById('company');
        comp.value = formData['company'];
        comp.dispatchEvent(new Event('change'));

        // 2. Tunggu sebentar sampai Region terisi, lalu isi Region
        setTimeout(() => {
            if (formData['region']) {
                const reg = document.getElementById('region');
                reg.value = formData['region'];
                reg.dispatchEvent(new Event('change'));

                // 3. Tunggu lagi sampai Lokasi terisi, lalu isi Lokasi
                setTimeout(() => {
                    if (formData['lokasi']) {
                        const lok = document.getElementById('lokasi');
                        $(lok).val(formData['lokasi']).trigger('change'); // Pakai trigger Select2
                    }
                    cekDataLengkap();
                }, 300);
            }
        }, 300);
    }
        
    // 4. Isi field lainnya (Nama, Email, HP, dsb)
    Object.keys(formData).forEach(id => {
        if (id !== 'company' && id !== 'region' && id !== 'lokasi') {
            const el = document.getElementById(id);
            if (el) el.value = formData[id];
        }
    });
}

// 3. Fungsi menghapus draft setelah sukses submit
function clearDraft() {
    localStorage.removeItem('form_magang_draft');
}

window.onload = cekDataLengkap;