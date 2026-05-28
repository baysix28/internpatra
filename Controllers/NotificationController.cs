using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using sinta_asp.Data;
using Microsoft.EntityFrameworkCore;

namespace sinta_asp.Controllers
{
    [Authorize(Policy = "PesertaOnly")]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userEmail = User.Identity?.Name;

            var notifications = await _context.Notifications
                .Where(n => n.UserEmail == userEmail)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt,
                    n.Url,
                    n.Type,
                    n.ExternalId
                })
                .ToListAsync();

            // Tambahkan timeAgo setelah query
            var result = notifications.Select(n => new {
                n.Id,
                n.Title,
                n.Message,
                n.IsRead,
                n.CreatedAt,
                n.Url,
                n.Type,
                n.ExternalId,
                timeAgo = GetTimeAgo(n.CreatedAt)
            });

            return Json(result);
        }

        [HttpGet]
        public IActionResult GoTo(int id)
        {
            var notif = _context.Notifications.FirstOrDefault(n => n.Id == id);

            if (notif == null)
                return RedirectToAction("Index", "DashboardPeserta");

            notif.IsRead = true;
            _context.SaveChanges();

            if (!string.IsNullOrEmpty(notif.Url))
            {
                // Jika Url sudah punya highlight parameter, langsung redirect
                if (notif.Url.Contains("highlight="))
                    return Redirect(notif.Url);

                // Jika ada ExternalId (ID magang), tambahkan parameter highlight
                if (!string.IsNullOrEmpty(notif.ExternalId))
                {
                    var baseUrl = notif.Url.Contains("?")
                        ? notif.Url + "&highlight=" + notif.ExternalId
                        : notif.Url + "?highlight=" + notif.ExternalId;

                    return Redirect(baseUrl);
                }

                return Redirect(notif.Url);
            }

            return Redirect("/DashboardPeserta?tab=riwayat");
        }

        [HttpPost]
        public IActionResult MarkAllRead()
        {
            var userEmail = User.Identity?.Name;

            var notifs = _context.Notifications
                .Where(n => n.UserEmail == userEmail && !n.IsRead)
                .ToList();

            foreach (var n in notifs)
                n.IsRead = true;

            _context.SaveChanges();
            return Ok();
        }

        // --- TAMBAHAN BARU: Hapus Notifikasi Spesifik ---
        [HttpPost]
        public IActionResult DeleteNotification(int id)
        {
            var userEmail = User.Identity?.Name;
            var notif = _context.Notifications.FirstOrDefault(n => n.Id == id && n.UserEmail == userEmail);

            if (notif == null)
                return NotFound();

            _context.Notifications.Remove(notif);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IActionResult UnreadCount()
        {
            var userEmail = User.Identity?.Name;

            var count = _context.Notifications
                .Count(n => n.UserEmail == userEmail && !n.IsRead);

            return Json(count);
        }

        // ─── Helper ────────────────────────────────────────────────
        private static string GetTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.Now - createdAt;

            if (diff.TotalMinutes < 1)    return "Baru saja";
            if (diff.TotalMinutes < 60)   return $"{(int)diff.TotalMinutes} menit lalu";
            if (diff.TotalHours < 24)     return $"{(int)diff.TotalHours} jam lalu";
            if (diff.TotalDays < 7)       return $"{(int)diff.TotalDays} hari lalu";
            if (diff.TotalDays < 30)      return $"{(int)(diff.TotalDays / 7)} minggu lalu";

            return createdAt.ToString("dd MMM yyyy");
        }
    }
}