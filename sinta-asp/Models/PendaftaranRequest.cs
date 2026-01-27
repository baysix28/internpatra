using System.ComponentModel.DataAnnotations;

namespace sinta_asp.Models
{
    public class PendaftaranRequest
    {
        // ========== DATA DIRI ==========
        [Required(ErrorMessage = "Foto wajib diupload")]
        public IFormFile? Foto { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
        public string? NamaLengkap { get; set; }

        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        public string? EmailPribadi { get; set; }

        [Required(ErrorMessage = "Tempat lahir wajib diisi")]
        [StringLength(50, ErrorMessage = "Tempat lahir maksimal 50 karakter")]
        public string? TempatLahir { get; set; }

        [Required(ErrorMessage = "Tanggal lahir wajib diisi")]
        [DataType(DataType.Date)]
        public DateTime? TanggalLahir { get; set; }

        [Required(ErrorMessage = "No HP wajib diisi")]
        [Phone(ErrorMessage = "Format nomor HP tidak valid")]
        [RegularExpression(@"^08[0-9]{8,11}$", ErrorMessage = "Nomor HP harus diawali 08 dan 10-13 digit")]
        public string? NoHp { get; set; }

        [Required(ErrorMessage = "Instagram wajib diisi")]
        [RegularExpression(@"^@[a-zA-Z0-9._]{1,30}$", ErrorMessage = "Format Instagram tidak valid (contoh: @username)")]
        public string? Instagram { get; set; }

        // ========== DATA KAMPUS ==========
        [Required(ErrorMessage = "Nama perguruan tinggi wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama perguruan tinggi maksimal 100 karakter")]
        public string? NamaPerguruanTinggi { get; set; }

        [Required(ErrorMessage = "Fakultas wajib diisi")]
        [StringLength(100, ErrorMessage = "Fakultas maksimal 100 karakter")]
        public string? Fakultas { get; set; }

        [Required(ErrorMessage = "Jurusan wajib diisi")]
        [StringLength(100, ErrorMessage = "Jurusan maksimal 100 karakter")]
        public string? Jurusan { get; set; }

        [Required(ErrorMessage = "NIM wajib diisi")]
        [StringLength(20, ErrorMessage = "NIM maksimal 20 karakter")]
        public string? NIM { get; set; }

        // ========== DATA MAGANG ==========
        [Required(ErrorMessage = "Company wajib dipilih")]
        public string? Company { get; set; }

        [Required(ErrorMessage = "Region wajib dipilih")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "Lokasi/Fungsi wajib dipilih")]
        public string? Lokasi { get; set; }

        // Opsional
        [StringLength(200, ErrorMessage = "Rekomendasi maksimal 200 karakter")]
        public string? RekomendasiPegawai { get; set; }

        [Required(ErrorMessage = "Tanggal mulai magang wajib diisi")]
        [DataType(DataType.Date)]
        public DateTime? MulaiMagang { get; set; }

        [Required(ErrorMessage = "Tanggal selesai magang wajib diisi")]
        [DataType(DataType.Date)]
        public DateTime? SelesaiMagang { get; set; }
        
        // ========== FILE PENDUKUNG ==========
        [Required(ErrorMessage = "CV wajib diupload")]
        public IFormFile? FileCV { get; set; }

        [Required(ErrorMessage = "Surat pengantar wajib diupload")]
        public IFormFile? FileSuratPengantar { get; set; }

        [Required(ErrorMessage = "Proposal wajib diupload")]
        public IFormFile? FileProposal { get; set; }

        // ========== VALIDASI CUSTOM ==========
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validasi tanggal selesai harus setelah tanggal mulai
            if (MulaiMagang.HasValue && SelesaiMagang.HasValue)
            {
                if (SelesaiMagang <= MulaiMagang)
                {
                    results.Add(new ValidationResult(
                        "Tanggal selesai harus setelah tanggal mulai",
                        new[] { nameof(SelesaiMagang) }
                    ));
                }
            }

            // Validasi ukuran file foto (max 2MB)
            if (Foto != null && Foto.Length > 2 * 1024 * 1024)
            {
                results.Add(new ValidationResult(
                    "Ukuran foto maksimal 2MB",
                    new[] { nameof(Foto) }
                ));
            }

            // Validasi file PDF (CV, Surat, Proposal)
            var pdfFiles = new[] 
            { 
                (FileCV, nameof(FileCV)), 
                (FileSuratPengantar, nameof(FileSuratPengantar)), 
                (FileProposal, nameof(FileProposal)) 
            };

            foreach (var (file, propertyName) in pdfFiles)
            {
                if (file != null)
                {
                    // Validasi ekstensi PDF
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (extension != ".pdf")
                    {
                        results.Add(new ValidationResult(
                            $"{propertyName.Replace("File", "")} harus berformat PDF",
                            new[] { propertyName }
                        ));
                    }

                    // Validasi ukuran max 5MB
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        results.Add(new ValidationResult(
                            $"{propertyName.Replace("File", "")} maksimal 5MB",
                            new[] { propertyName }
                        ));
                    }
                }
            }

            return results;
        }
    }
}