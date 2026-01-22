using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace sinta_asp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminHomeController : Controller
    {
        public IActionResult Index()
        {
            // CEK LOGIN ADMIN
            if (HttpContext.Session.GetString("AdminLogin") != "true")
            {
                return RedirectToAction("Index", "Login", new { area = "Admin" });
            }

            return View();
        }
    }
}
