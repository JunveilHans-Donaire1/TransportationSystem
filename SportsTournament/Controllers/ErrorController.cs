using Microsoft.AspNetCore.Mvc;
using SportsTournament.Models;
using SportsTournament.ViewModels;
using System.Diagnostics;

namespace VotingSystemProject.Controllers
{
    public class ErrorController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
