using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using sinta_asp.Models;
using sinta_asp.Data;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace sinta_asp.Controllers
{
    public class PenelitianController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PenelitianController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private List<Lowongan> GetDummyData()
        {
            return new List<Lowongan>
            {
                new Lowongan {
                    Title = "Akuntansi/ Ekonomi & Bisnis",
                    Region = "Refinery Unit VI Balongan",
                    Company = "PT Kilang Pertamina Internasional (KPI)",
                    ImageUrl = "https://images.unsplash.com/photo-1581092921461-eab62e97a782?w=400",
                    Description = "Mempelajari proses distilasi minyak mentah dan monitoring unit operasi di kilang Balongan untuk menjaga kualitas produk BBM.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan {
                    Title = "Elektro (Arus Kuat)",
                    Region = "Refinery Unit VI Balongan",
                    Company = "PT Kilang Pertamina Internasional (KPI)",
                    ImageUrl = "https://images.unsplash.com/photo-1504328345606-18bbc8c9d7d1?w=400",
                    Description = "Fokus pada pemeliharaan dan analisis performa mesin rotasi seperti pompa, kompresor, dan turbin di area kilang.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan {
                    Title = "Asset Operation MOR V",
                    Region = "Regional Jatimbalinus",
                    Company = "PT Pertamina Patra Niaga (C&T)",
                    ImageUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=400",
                    Description = "Terminal BBM strategis dan terpenting di Indonesia yang menyuplai kebutuhan energi untuk wilayah Jabodetabek.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan {
                    Title = "Asset Operation JBB",
                    Region = "Regional Jawa Bagian Barat",
                    Company = "PT Pertamina Patra Niaga (C&T)",
                    ImageUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=400",
                    Description = "Terminal BBM strategis dan terpenting di Indonesia yang menyuplai kebutuhan energi untuk wilayah Jabodetabek.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan {
                    Title = "Aviation FT Babullah",
                    Region = "Regional Maluku Papua",
                    Company = "PT Pertamina Patra Niaga (C&T)",
                    ImageUrl = "https://images.unsplash.com/photo-1556761175-5973dc0f32e7?w=400",
                    Description = "Depot Pengisian Pesawat Udara (DPPU) tersibuk kedua di Indonesia, melayani avtur untuk penerbangan internasional.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan {
                    Title = "Kantor Unit - SSC ICT V JBT",
                    Region = "Regional Jawa Bagian Tengah",
                    Company = "PT Pertamina Patra Niaga (C&T)",
                    ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400",
                    Description = "Mendukung operasional IT dan infrastruktur jaringan untuk kelancaran distribusi energi di Jawa Tengah.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan {
                    Title = "Asset Operation Region Sumbagut - Kantor Unit",
                    Region = "Regional Sumbagut",
                    Company = "PT Pertamina Patra Niaga (C&T)",
                    ImageUrl = "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=400",
                    Description = "Menangani aspek legalitas aset dan hubungan industrial di salah satu terminal BBM vital di Kalimantan.",
                    CreatedAt = DateTime.Now
                },
                new Lowongan {
                    Title = "DPPU APT Pranoto",
                    Region = "Regional Kalimantan",
                    Company = "PT Pertamina Patra Niaga (C&T)",
                    ImageUrl = "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=400",
                    Description = "Menangani aspek legalitas aset dan hubungan industrial di salah satu terminal BBM vital di Kalimantan.",
                    CreatedAt = DateTime.Now
                }
            };
        }

        public IActionResult Index(string search, string company, string region, int page = 1)
        {
            if (!_context.Lowongan.Any())
            {
                var dataDummy = GetDummyData();
                _context.Lowongan.AddRange(dataDummy);
                _context.SaveChanges();
            }

            var allData = _context.Lowongan.ToList();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                allData = allData.Where(x => (x.Title != null && x.Title.ToLower().Contains(search)) ||
                                            (x.Company != null && x.Company.ToLower().Contains(search))).ToList();
            }

            if (!string.IsNullOrEmpty(company) && company != "All")
                allData = allData.Where(x => x.Company != null && x.Company.Contains(company)).ToList();

            if (!string.IsNullOrEmpty(region) && region != "All")
                allData = allData.Where(x => x.Region == region).ToList();

            int pageSize = 8;
            int totalItems = allData.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var dataHalamanIni = allData
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var model = new sinta_asp.Models.LowonganViewModel
            {
                Lowongan = dataHalamanIni,
                CurrentPage = page,
                TotalPages = totalPages
            };

            ViewData["SelectedSearch"] = search;
            ViewData["SelectedCompany"] = company;
            ViewData["SelectedRegion"] = region;

            return View(model);
        }

        public IActionResult Daftar()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                ViewData["ShowLoginAlert"] = true;

            var model = new sinta_asp.Models.PendaftaranPenelitianModel();
            return View(model);
        }
        [HttpGet]
        public IActionResult Sukses()
        {
            ViewBag.NomorPendaftaran = TempData["NomorPendaftaran"];
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var data = await _context.Pendaftarans
                .FirstOrDefaultAsync(p => p.Id == id);

            if (data == null)
                return Json(new { success = false, message = "Data tidak ditemukan" });

            var userEmail = User.Identity?.Name;
            var profil = await _context.UserProfile
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return Json(new {
                nama               = profil?.NamaLengkap ?? data.Nama,
                email              = data.Email,
                universitas        = data.Universitas,
                nim                = data.Nim,
                jurusan            = data.Jurusan,
                fakultas           = data.Fakultas,
                company            = data.Company,
                lokasiPenelitian   = data.LokasiPenelitian,
                judulPenelitian    = data.JudulPenelitian,
                tempatLahir        = data.TempatLahir,
                tglLahir           = data.TglLahir?.ToString("dd MMM yyyy"),
                noHp               = data.NoHp,
                instagram          = data.Instagram,
                tglMulai           = data.TglMulai?.ToString("dd MMM yyyy"),
                tglSelesai         = data.TglSelesai?.ToString("dd MMM yyyy"),
                createdAtFormatted = data.CreatedAt.ToString("dd MMM yyyy"),
                status             = data.Status,
                pathFoto           = !string.IsNullOrEmpty(profil?.FotoProfil)
                                    ? "/uploads/profile/" + profil.FotoProfil : null,
                pathCV             = data.PathCV ?? "#",
                pathSurat          = data.PathSurat ?? "#",
                pathProposal       = data.PathProposal ?? "#"
            });
        }
        [HttpPost]
        public async Task<IActionResult> SubmitPendaftaran(PendaftaranPenelitianModel model)
        {
            var pesertaResult = await HttpContext.AuthenticateAsync("PesertaScheme");
            if (!pesertaResult.Succeeded)
                return RedirectToAction("Index");

            ModelState.Remove("NomorPendaftaran");
            ModelState.Remove("Status");
            ModelState.Remove("TargetLokasi");
            ModelState.Remove("TargetJurusan");
            ModelState.Remove("PathFoto3x4");
            ModelState.Remove("PathCV");
            ModelState.Remove("PathProposal");
            ModelState.Remove("PathSurat");

            if (!ModelState.IsValid)
            {
                var errorFields = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .Select(x => x.Key + ": " + string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage)));
                return Content("FIELD GAGAL: " + string.Join(" | ", errorFields));
            }

            string fotoPath = await UploadFile(model.Foto3x4, "foto");
            string cvPath = await UploadFile(model.FileCV, "cv");
            string proposalPath = await UploadFile(model.FileProposal, "proposal");
            string suratPath = await UploadFile(model.FileSurat, "surat");

            string nomorGenerated = GenerateNomorPendaftaran();

            var pendaftaranBaru = new Pendaftaran
            {
                NomorPendaftaran = nomorGenerated,
                Nama = model.Nama,
                Email = model.Email,
                NoHp = model.NoHp,
                TempatLahir = model.TempatLahir,
                TglLahir = model.TglLahir,
                Instagram = model.Instagram,
                PathFoto3x4 = fotoPath,
                Universitas = model.Universitas,
                Fakultas = model.Fakultas,
                Jurusan = model.Jurusan,
                Nim = model.Nim,
                Company = model.Company,
                Region = model.Region,
                LokasiPenelitian = model.LokasiPenelitian,
                JudulPenelitian = model.JudulPenelitian,
                TglMulai = model.TglMulai,
                TglSelesai = model.TglSelesai,
                TargetLokasi = model.TargetLokasi,
                TargetJurusan = model.TargetJurusan,
                PathCV = cvPath,
                PathProposal = proposalPath,
                PathSurat = suratPath,
                CreatedAt = DateTime.Now,
                Status = "Dalam Proses"
            };

            _context.Pendaftarans.Add(pendaftaranBaru);
            await _context.SaveChangesAsync();

            // Notifikasi untuk Admin
            var notifAdmin = new AdminNotification
            {
                Title        = "Pendaftaran Penelitian Baru",
                Message      = $"{pendaftaranBaru.Nama} dari {pendaftaranBaru.Universitas} mendaftar penelitian di {pendaftaranBaru.Region}.",
                Type         = "penelitian",
                TargetRegion = pendaftaranBaru.Region,
                CreatedAt    = DateTime.Now,
                IsRead       = false,
                MagangId     = pendaftaranBaru.Id
            };
            _context.AdminNotifications.Add(notifAdmin);
            await _context.SaveChangesAsync();

            KirimEmailNotifikasi(model.Email, nomorGenerated, model.Nama);

            TempData["NomorPendaftaran"] = nomorGenerated;
            return RedirectToAction("Sukses", "Penelitian");
        }
        [HttpGet]
        public IActionResult CekStatus()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> CekAuth()
        {
            var peserta = await HttpContext.AuthenticateAsync("PesertaScheme");
            return Content($"Succeeded: {peserta.Succeeded} | Name: {peserta.Principal?.Identity?.Name} | Claims: {string.Join(", ", peserta.Principal?.Claims?.Select(c => c.Type + "=" + c.Value) ?? new List<string>())}");
        }

        [HttpPost]
        public IActionResult CekStatus(string noPendaftaran)
        {
            var data = _context.Pendaftarans.FirstOrDefault(x => x.NomorPendaftaran == noPendaftaran);

            if (data == null)
                ViewBag.PesanError = "Nomor Pendaftaran tidak ditemukan. Silakan periksa kembali.";

            return View(data);
        }

        private async Task<string?> UploadFile(Microsoft.AspNetCore.Http.IFormFile file, string jenis)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", jenis);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + jenis + "/" + uniqueFileName;
        }

        private string GenerateNomorPendaftaran()
        {
            DateTime now = DateTime.Now;
            string tahun = now.Year.ToString();
            string bulanRomawi = GetBulanRomawi(now.Month);
            string prefix = $"PEN/{tahun}/{bulanRomawi}/";

            var dataTerakhir = _context.Pendaftarans
                                .Where(x => x.NomorPendaftaran.StartsWith(prefix))
                                .OrderByDescending(x => x.NomorPendaftaran)
                                .FirstOrDefault();

            int urutan = 1;
            if (dataTerakhir != null)
            {
                string[] parts = dataTerakhir.NomorPendaftaran.Split('/');
                string angkaTerakhir = parts[parts.Length - 1];

                if (int.TryParse(angkaTerakhir, out int lastNumber))
                    urutan = lastNumber + 1;
            }

            return $"{prefix}{urutan.ToString("D4")}";
        }


        private string GetBulanRomawi(int bulan)
        {
            string[] romawi = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII" };
            return (bulan >= 1 && bulan <= 12) ? romawi[bulan] : "";
        }

        private void KirimEmailNotifikasi(string emailTujuan, string noPendaftaran, string nama)
        {
            try
            {
                string emailPengirim = "sintapertamina@gmail.com";
                string passwordApp = "cipjzsmrwrwhvtnv";

                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(emailPengirim, passwordApp);

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(emailPengirim, "Sistem Internship Pertamina");
                mail.To.Add(emailTujuan);
                mail.Subject = "Pendaftaran Magang Berhasil - ";

                string bodyEmail = $@"
                    <p>Yth. Sdr/i <b>{nama}</b>,</p>
                    <p>Pendaftaran penelitian Anda telah masuk dalam sistem dengan nomor pendaftaran:</p>
                    <p><b>{noPendaftaran}</b></p>
                    <p>Silakan tunggu email tanggapan dari kami atau periksa status penerimaan penelitian Anda melalui Web Sinta dengan memasukkan nomor pendaftaran tersebut.</p>
                    <p>
                        Salam hormat,<br/>
                        Human Capital<br/>
                        PT Pertamina Patra Niaga Regional Jawa Bagian Tengah
                    </p>
                    <hr/>
                    <p style='font-size: 11px; color: gray;'>*Email ini dikirimkan secara otomatis, mohon untuk <b>tidak membalas (do not reply)</b> email ini.</p>
                ";

                mail.Body = bodyEmail;
                mail.IsBodyHtml = true;

                client.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gagal kirim email: " + ex.Message);
            }
        }
    }
}