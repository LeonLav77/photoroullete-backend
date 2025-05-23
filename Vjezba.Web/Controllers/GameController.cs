using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vjezba.DAL;
using Vjezba.Model;

namespace Vjezba.Web.Controllers
{
    public class GameController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ClientManagerDbContext>();
                optionsBuilder.UseSqlite("Data Source=ClientManager.db");
                
                using var context = new ClientManagerDbContext(optionsBuilder.Options);
                
                Console.WriteLine("Loading games from database...");
                
                // First, let's see how many games are in the database
                var gameCount = await context.Games.CountAsync();
                Console.WriteLine($"Total games in database: {gameCount}");
                
                var games = await context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                    .ThenInclude(r => r.AnswersCollection)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"Loaded {games.Count} games successfully");
                
                foreach (var game in games)
                {
                    Console.WriteLine($"Game: {game.Code}, Players: {game.PlayersCollection.Count}, Rounds: {game.RoundsCollection.Count}, Created: {game.CreatedAt}");
                }

                return View(games);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading games: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return View(new List<Game>());
            }
        }

        public async Task<IActionResult> GameFlow(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ClientManagerDbContext>();
                optionsBuilder.UseSqlite("Data Source=ClientManager.db");
                
                using var context = new ClientManagerDbContext(optionsBuilder.Options);

                var game = await context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                    .ThenInclude(r => r.AnswersCollection)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (game == null)
                {
                    return NotFound();
                }

                // Calculate player scores to avoid LINQ in view
                var playerScores = new Dictionary<string, int>();
                foreach (var player in game.PlayersCollection)
                {
                    var score = game.RoundsCollection
                        .SelectMany(r => r.AnswersCollection)
                        .Where(a => a.Player == player.ConnectionId)
                        .Sum(a => a.Score);
                    playerScores[player.ConnectionId] = score;
                }

                ViewBag.PlayerScores = playerScores;
                return View(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game {id}: {ex.Message}");
                return NotFound();
            }
        }
    }
}