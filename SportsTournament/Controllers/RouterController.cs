using Microsoft.AspNetCore.Mvc;

namespace SportsTournament.Controllers
{
    public class RouterController : Controller
    {
        public IActionResult Router()
        {
            return View();
        }
    }
}
