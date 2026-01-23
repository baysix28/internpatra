using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using sinta_asp.Areas.Admin.Models;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // CEK LOGIN
            if (HttpContext.Session.GetString("AdminLogin") != "true")
            {
                return RedirectToAction("Index", "Login", new { area = "Admin" });
            }

            var model = new DashboardModel
            {
                AdminName = HttpContext.Session.GetString("AdminName"),
                LoginTime = DateTime.Now
            };

            return View(model);
        }
    }
}
