using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using sinta_asp.Data;
using System.Linq;

namespace sinta_asp.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult GetUserNotifications()
        {
            var userEmail = User.Identity?.Name;
            Console.WriteLine("EMAIL YANG DIBACA CONTROLLER: " + userEmail);


            var notifications = _context.Notifications
                .Where(n => n.UserEmail == userEmail)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt,
                    n.Url
                })
                .ToList();

            return Json(notifications);
        }

        public IActionResult GoTo(int id)
        {
            var notif = _context.Notifications.FirstOrDefault(n => n.Id == id);

            if (notif == null)
                return RedirectToAction("Index", "DashboardPeserta");

            // tandai dibaca
            notif.IsRead = true;
            _context.SaveChanges();

            // redirect ke url notif
            if (!string.IsNullOrEmpty(notif.Url))
                return Redirect(notif.Url);

            return RedirectToAction("Index", "DashboardPeserta");
        }

        public IActionResult MarkAllRead()
        {
            var userEmail = User.Identity?.Name;

            var notifs = _context.Notifications
                .Where(n => n.UserEmail == userEmail && !n.IsRead)
                .ToList();

            foreach (var n in notifs)
            {
                n.IsRead = true;
            }

            _context.SaveChanges();

            return Ok();
        }

        public IActionResult UnreadCount()
        {
            var userEmail = User.Identity?.Name;

            var count = _context.Notifications
                .Count(n => n.UserEmail == userEmail && !n.IsRead);

            return Json(count);
        }
    }
}