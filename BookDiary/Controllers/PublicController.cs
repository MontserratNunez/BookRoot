using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BookDiary.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("Terms")]
        public IActionResult Terms()
        {
            return View();
        }

        [HttpGet("Attributions")]
        public IActionResult Attributions()
        {
            return View();
        }
    }
}
