using Microsoft.AspNetCore.Mvc;

namespace Userstories.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult ChooseRole()
        {
            return View();
        }
        public IActionResult Signup()
        {
            return View();
        }
    }
}
