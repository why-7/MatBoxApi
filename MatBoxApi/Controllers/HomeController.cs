using Microsoft.AspNetCore.Mvc;

namespace MatBoxApi.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        // GET
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}