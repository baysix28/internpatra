// ===== Email =====
function validateEmail(input) {
    const emailError = document.getElementById('email-error');
    const emailPattern = /^[a-zA-Z0-9._%+-]+@@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    if (input.value === "") {
        input.classList.remove('is-invalid');
        emailError.style.display = 'none';
    } else if (!emailPattern.test(input.value)) {
        input.classList.add('is-invalid');
        emailError.style.display = 'flex';
    } else {
        input.classList.remove('is-invalid');
        emailError.style.display = 'none';
    }
}

// ===== Tanggal Periode Magang =====
function validasiPeriodeMagang() {
    const tglMulai = document.getElementById('tanggal_mulai');
    const tglSelesai = document.getElementById('SelesaiMagang');
    const errorMsg = document.getElementById('error-selesai');

    if (tglMulai.value && tglSelesai.value) {
        const mulai = new Date(tglMulai.value);
        const selesai = new Date(tglSelesai.value);

        if (selesai <= mulai) {
            tglSelesai.classList.add('is-invalid');
            errorMsg.style.display = 'block';
        } else {
            tglSelesai.classList.remove('is-invalid');
            errorMsg.style.display = 'none';
        }
    }

    if (typeof cekDataLengkap === "function") {
        cekDataLengkap();
    }
}

// Pasang listener pada kedua input tanggal
document.getElementById('tanggal_mulai')?.addEventListener('change', validasiPeriodeMagang);
document.getElementById('SelesaiMagang')?.addEventListener('change', validasiPeriodeMagang);


// Validasi Foto (max 2MB)
let fotoTerisi = false;
document.getElementById('foto').addEventListener('change', function () {
    const file = this.files[0];
    const errorMsg = document.getElementById('error-foto');
    const uploadBox = document.getElementById('uploadBox');
    const prev = document.getElementById('preview');
    const place = document.getElementById('placeholder');

    if (file) {
        // Jika ukuran lebih dari 2MB
        if (file.size > 2 * 1024 * 1024) {
            // Tampilkan error di page
            errorMsg.style.display = 'flex';
            uploadBox.classList.add('is-invalid');
                
            // Reset input dan preview
            this.value = '';
            prev.style.display = 'none';
            place.style.display = 'block';
            fotoTerisi = false;
        } else {
            // Jika valid, sembunyikan error
            errorMsg.style.display = 'none';
            uploadBox.classList.remove('is-invalid');

            const reader = new FileReader();
            reader.onload = e => {
                prev.src = e.target.result;
                prev.style.display = 'block';
                place.style.display = 'none';
                fotoTerisi = true;
                if (typeof cekDataLengkap === "function") cekDataLengkap();
            };
            reader.readAsDataURL(file);
        }
    }
});

// Validasi File PDF 
const fileInputs = ['file_cv', 'file_surat_pengantar', 'file_transkrip'];

fileInputs.forEach(id => {
    const input = document.getElementById(id);
    const errorMsg = document.getElementById('error-' + id);

    if (input) {
        input.addEventListener('change', function() {
            const file = this.files[0];
            const maxMB = 5;
            const maxSize = maxMB * 1024 * 1024; // 5MB

            if (file && file.size > maxSize) {
                // Tampilkan gaya error merah (sama seperti foto)
                errorMsg.style.display = 'flex';
                this.classList.add('is-invalid');
                    
                // Reset input
                this.value = '';
            } else {
                // Sembunyikan error jika file aman
                errorMsg.style.display = 'none';
                this.classList.remove('is-invalid');
            }
            // Update titik merah di stepper
            cekDataLengkap();
        });
    }
});