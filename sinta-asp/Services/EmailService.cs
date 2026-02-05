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
        // 1. EMAIL KURIR → ADMIN (Untuk Pendaftaran Baru)
        // ============================================================
        public async Task SendWithCourierAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("SINTA System", _config["EmailSettings:Email"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    _config["EmailSettings:Host"], 
                    int.Parse(_config["EmailSettings:Port"]), 
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
        // 3. NOTIFIKASI SELESAI MAGANG → ADMIN REGION (Baru)
        // Memberitahu Admin bahwa ada peserta di regionnya yang selesai hari ini
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

            // Menggunakan fungsi kurir pusat untuk mengirim ke email admin region
            await SendWithCourierAsync(adminEmail, subject, body);
        }
    }
}