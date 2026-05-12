using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace sinta_asp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // ============================================================
        // 1. EMAIL KURIR → ADMIN (Diperbarui dengan displayName)
        // ============================================================
        public async Task SendWithCourierAsync(string to, string subject, string body, string? displayName = null)
        {
            var email = new MimeMessage();
            
            // Gunakan displayName jika ada, jika tidak gunakan default "SINTA System"
            string nameToShow = string.IsNullOrEmpty(displayName) ? "SINTA System" : displayName;
            
            email.From.Add(new MailboxAddress(nameToShow, _config["EmailSettings:Email"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    _config["EmailSettings:Host"], 
                    int.Parse(_config["EmailSettings:Port"]?? "587"), 
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _config["EmailSettings:Email"], 
                    _config["EmailSettings:Password"]
                );

                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        // ============================================================
        // 2. EMAIL ADMIN REGION → PENDAFTAR (Untuk Update Status)
        // ============================================================
        public async Task SendAsAdminAsync(string fromEmail, string smtpPassword, string to, string subject, string body, string displayName)
        {
            if (string.IsNullOrEmpty(smtpPassword))
                throw new Exception("Konfigurasi SMTP Admin (App Password) tidak ditemukan.");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(displayName, fromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(fromEmail.Trim(), smtpPassword.Trim());
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Gagal kirim email via Admin {fromEmail}: {ex.Message}");
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        // ============================================================
        // 3. NOTIFIKASI SELESAI MAGANG → ADMIN REGION
        // ============================================================
        public async Task SendCompletionNotificationToAdminAsync(string adminEmail, string namaPeserta, string lokasi)
        {
            var subject = $"[Notifikasi Selesai] Peserta Magang: {namaPeserta}";
            var body = $@"
                <h3>Halo Admin {lokasi},</h3>
                <p>Memberitahukan bahwa peserta magang berikut telah mencapai tanggal selesai program pada hari ini:</p>
                <ul>
                    <li><strong>Nama:</strong> {namaPeserta}</li>
                    <li><strong>Lokasi:</strong> {lokasi}</li>
                    <li><strong>Tanggal Selesai:</strong> {DateTime.Now:dd MMMM yyyy}</li>
                </ul>
                <p>Mohon segera melakukan pengecekan data dan memproses administrasi/sertifikat yang diperlukan di dashboard SINTA.</p>
                <br>
                <p>Salam,<br><strong>SINTA System Notifier</strong></p>";

            // Tetap memanggil dengan 3 parameter (displayName akan menjadi null/default)
            await SendWithCourierAsync(adminEmail, subject, body);
        }

        // ============================================================
        // 4. RESET PASSWORD → ADMIN (Fitur Lupa Password)
        // ============================================================
        public async Task SendForgotPasswordEmailAsync(string toEmail, string code)
        {
            var subject = "Kode Reset Password SINTA Admin";
            var body = $@"
                <h3>Permintaan Reset Password</h3>
                <p>Kami menerima permintaan untuk mereset password akun Admin SINTA Anda.</p>
                <p>Gunakan kode berikut untuk melanjutkan:</p>
                
                <div style='margin: 20px 0; text-align:center;'>
                    <span style='font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #007bff;'>{code}</span>
                </div>

                <p>Kode ini akan kedaluwarsa dalam 1 jam.</p>
                <p>Jika Anda tidak merasa melakukan permintaan ini, silakan abaikan email ini.</p>
                <br>
                <p>Salam,<br><strong>SINTA System</strong></p>";

            await SendWithCourierAsync(toEmail, subject, body);
        }
    }
}