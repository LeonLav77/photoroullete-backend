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

        // POST /home/sendbroadcast
        [HttpPost]
        public async Task<IActionResult> SendBroadcast()
        {
            var message = "Hello from the server!";
            await _hubContext.Clients.All.SendAsync("StartRoom", message);
            
            // Return a result that won't reload the page
            return Json(new { success = true });
        }
    }
}