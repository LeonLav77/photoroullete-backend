using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vjezba.DAL;
using Vjezba.Model;
using Vjezba.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace Vjezba.Web.Controllers
{
    public class GameController : Controller
    {
        private readonly GameValidationService _validationService;

        public GameController()
        {
            _validationService = new GameValidationService();
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ClientManagerDbContext>();
                optionsBuilder.UseSqlite("Data Source=ClientManager.db");

                using var context = new ClientManagerDbContext(optionsBuilder.Options);

                Console.WriteLine("Loading games from database...");

                var gameCount = await context.Games.CountAsync();

                var games = await context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                    .ThenInclude(r => r.AnswersCollection)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();

                return View(games);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading games: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return View(new List<Game>());
            }
        }

        [Authorize(Roles = "Manager,Admin")]
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

        [HttpGet]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> EditGame(int id)
        {
            var idValidation = _validationService.ValidateGameId(id);
            if (!idValidation.IsValid)
            {
                return BadRequest(idValidation.ErrorMessage);
            }

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ClientManagerDbContext>();
                optionsBuilder.UseSqlite("Data Source=ClientManager.db");

                using var _context = new ClientManagerDbContext(optionsBuilder.Options);

                var game = await _context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                        .ThenInclude(r => r.AnswersCollection)
                    .FirstOrDefaultAsync(g => g.Id == id);

                var gameValidation = _validationService.ValidateGameData(game, id);
                if (!gameValidation.IsValid)
                {
                    return gameValidation.StatusCode == 404
                        ? NotFound(gameValidation.ErrorMessage)
                        : BadRequest(gameValidation.ErrorMessage);
                }

                var playerScores = new Dictionary<string, int>();
                foreach (var player in game.PlayersCollection)
                {
                    if (!_validationService.IsValidPlayer(player))
                    {
                        continue;
                    }

                    var totalScore = game.RoundsCollection
                        .SelectMany(r => r.AnswersCollection ?? new List<Answer>())
                        .Where(a => a.Player == player.ConnectionId)
                        .Sum(a => a.Score);
                    playerScores[player.ConnectionId] = totalScore;
                }

                ViewBag.PlayerScores = playerScores;
                return View(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game {id} for editing: {ex.Message}");
                return StatusCode(500, "An error occurred while loading the game");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> SaveGameChanges([FromBody] SaveGameChangesRequest request)
        {
            foreach (var answerScore in request.AnswerScores)
            {
                var scoreValidation = _validationService.ValidateScore(answerScore.Value);
                if (!scoreValidation.IsValid)
                {
                    return Json(new { success = false, message = scoreValidation.ErrorMessage });
                }
            }

            var optionsBuilder = new DbContextOptionsBuilder<ClientManagerDbContext>();
            optionsBuilder.UseSqlite("Data Source=ClientManager.db");

            using var _context = new ClientManagerDbContext(optionsBuilder.Options);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                if (request.Excitement.HasValue)
                {
                    var game = await _context.Games.FindAsync(request.GameId);
                    if (game != null)
                    {
                        game.Excitement = (GameExcitement)request.Excitement.Value;
                        _context.Games.Update(game);
                    }
                }

                foreach (var answerScore in request.AnswerScores)
                {
                    var answer = await _context.Answers.FindAsync(answerScore.Key);

                    if (answer != null)
                    {
                        answer.Score = answerScore.Value;
                        _context.Answers.Update(answer);
                    }
                }


                foreach (var roundId in request.DeletedRounds)
                {
                    var round = await _context.Rounds
                        .Include(r => r.AnswersCollection)
                        .FirstOrDefaultAsync(r => r.Id == roundId);

                    if (round != null)
                    {
                        _context.Answers.RemoveRange(round.AnswersCollection);

                        _context.Rounds.Remove(round);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Changes saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}