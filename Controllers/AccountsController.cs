using Microsoft.AspNetCore.Mvc;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
