using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Vjezba.DAL;

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

        public async Task<IActionResult> Index()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ClientManagerDbContext>();
                optionsBuilder.UseSqlite("Data Source=/var/www/vjezba/ClientManager.db");

                using var context = new ClientManagerDbContext(optionsBuilder.Options);

                ViewBag.TotalGames = await context.Games.CountAsync();
                ViewBag.TotalPlayers = await context.Players.CountAsync();
                ViewBag.TotalPhotos = await context.Rounds.CountAsync();
                ViewBag.TotalAnswers = await context.Answers.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating statistics for home page");
                
                ViewBag.TotalGames = "N/A";
                ViewBag.TotalPlayers = "N/A";
                ViewBag.TotalPhotos = "N/A";
                ViewBag.TotalAnswers = "N/A";
            }

            return View();
        }
    }
}