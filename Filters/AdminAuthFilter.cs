using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace sinta_asp.Areas.Admin.Filters
{
    public class AdminAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var isLoggedIn = session.GetString("AdminLogin");

            if (isLoggedIn != "true")
            {
                context.Result = new RedirectToActionResult("Index", "Login", new { area = "Admin" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}