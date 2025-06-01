using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Vjezba.Model;
using Vjezba.Web;  // Your RouletteHub namespace

namespace Vjezba.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<RouletteHub> _hubContext;

        public HomeController(
            ILogger<HomeController> logger,
            IHubContext<RouletteHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}