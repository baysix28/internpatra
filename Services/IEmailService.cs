using System.Threading.Tasks;

namespace sinta_asp.Services
{
    public interface IEmailService
    {
        // Untuk mengirim email dari sistem pusat ke admin (seperti pendaftaran baru)
        // DITAMBAHKAN: parameter ke-4 (displayName) sebagai optional agar tidak error CS1501
        Task SendWithCourierAsync(string to, string subject, string body, string? displayName = null);

        // Untuk mengirim email dari admin region ke peserta magang (update status)
        Task SendAsAdminAsync(string fromEmail, string smtpPassword, string to, string subject, string body, string displayName);

        // Tambahan: Untuk mengirim notifikasi otomatis ke Admin Region jika ada peserta yang selesai hari ini
        Task SendCompletionNotificationToAdminAsync(string adminEmail, string namaPeserta, string lokasi);

        // JANGAN UBAH DI ATAS - Tambahan untuk fitur Forgot Password
        Task SendForgotPasswordEmailAsync(string email, string resetLink);
    }
}