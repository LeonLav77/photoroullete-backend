using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Vjezba.Model;
using Vjezba.DAL;

namespace Vjezba.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameApiController : ControllerBase
    {
        private readonly ClientManagerDbContext _context;

        public GameApiController(ClientManagerDbContext context)
        {
            _context = context;
        }

        // GET: api/GameApi
        [HttpGet]
        public async Task<IActionResult> GetAllGames()
        {
            try
            {
                var games = await _context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                        .ThenInclude(r => r.AnswersCollection)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                return new JsonResult(games, options);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving games: {ex.Message}");
            }
        }

        // GET: api/GameApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            try
            {
                var game = await _context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                        .ThenInclude(r => r.AnswersCollection)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (game == null)
                {
                    return NotFound($"Game with ID {id} not found.");
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                return new JsonResult(game, options);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving game: {ex.Message}");
            }
        }



        // PUT: api/GameApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(int id, [FromBody] Game request)
        {
            try
            {
                var game = await _context.Games.FindAsync(id);
                if (game == null)
                {
                    return NotFound($"Game with ID {id} not found.");
                }

                // Check if new code conflicts with existing games (excluding current game)
                if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != game.Code)
                {
                    var existingGame = await _context.Games
                        .FirstOrDefaultAsync(g => g.Code == request.Code && g.Id != id);
                    if (existingGame != null)
                    {
                        return BadRequest($"Game with code '{request.Code}' already exists.");
                    }
                    game.Code = request.Code;
                }

                if (request.FinishedAt.HasValue)
                {
                    game.FinishedAt = request.FinishedAt.Value;
                }

                // Update excitement level
                game.Excitement = request.Excitement;

                _context.Games.Update(game);
                await _context.SaveChangesAsync();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                return new JsonResult(game, options);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating game: {ex.Message}");
            }
        }

        // DELETE: api/GameApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            try
            {
                var game = await _context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                        .ThenInclude(r => r.AnswersCollection)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (game == null)
                {
                    return NotFound($"Game with ID {id} not found.");
                }

                // Remove all related data (cascade delete)
                foreach (var round in game.RoundsCollection)
                {
                    _context.Answers.RemoveRange(round.AnswersCollection);
                }
                
                _context.Rounds.RemoveRange(game.RoundsCollection);
                _context.Players.RemoveRange(game.PlayersCollection);
                _context.Games.Remove(game);

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Game '{game.Code}' and all related data deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting game: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("/api/game/export/{gameId}")]
        public async Task<IActionResult> ExportGame(int gameId)
        {
            try
            {
                var game = await _context.Games
                    .Include(g => g.PlayersCollection)
                    .Include(g => g.RoundsCollection)
                        .ThenInclude(r => r.AnswersCollection)
                    .FirstOrDefaultAsync(g => g.Id == gameId);

                if (game == null)
                {
                    return NotFound($"Game with ID {gameId} not found.");
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                var jsonString = JsonSerializer.Serialize(game, options);
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

                return File(bytes, "application/json", $"game_{game.Code}_export.json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error exporting game: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("/api/game/import/{gameCode}")]
        public async Task<IActionResult> ImportGame(string gameCode, [FromBody] JsonElement jsonData)
        {
            try
            {
                if (jsonData.ValueKind == JsonValueKind.Undefined)
                {
                    return BadRequest("No game data provided.");
                }

                // Check if game with this code already exists
                var existingGame = await _context.Games.FirstOrDefaultAsync(g => g.Code == gameCode);
                if (existingGame != null)
                {
                    return BadRequest($"Game with code '{gameCode}' already exists. Please use a different code or delete the existing game first.");
                }

                var properties = new List<string>();
                foreach (var property in jsonData.EnumerateObject())
                {
                    properties.Add(property.Name);
                }

                // Use gameCode from URL parameter instead of JSON
                var createdAt = jsonData.GetProperty("createdAt").GetDateTime();
                var finishedAt = jsonData.GetProperty("finishedAt").GetDateTime();
                
                // Try to get excitement from JSON, default to Average if not present
                var excitement = GameExcitement.Average;
                if (jsonData.TryGetProperty("excitement", out var excitementProperty))
                {
                    if (excitementProperty.ValueKind == JsonValueKind.Number)
                    {
                        var excitementValue = excitementProperty.GetInt32();
                        if (Enum.IsDefined(typeof(GameExcitement), excitementValue))
                        {
                            excitement = (GameExcitement)excitementValue;
                        }
                    }
                }

                var gameEntity = new Game
                {
                    Code = gameCode, // Use URL parameter
                    CreatedAt = createdAt,
                    FinishedAt = finishedAt,
                    Excitement = excitement
                };

                _context.Games.Add(gameEntity);
                await _context.SaveChangesAsync();

                var playersCount = 0;
                var roundsCount = 0;

                JsonElement playersProperty;
                bool hasPlayers = jsonData.TryGetProperty("players", out playersProperty);

                if (hasPlayers)
                {
                    foreach (var playerJson in playersProperty.EnumerateArray())
                    {
                        var connectionId = playerJson.GetProperty("connectionId").GetString();
                        var name = playerJson.GetProperty("name").GetString();
                        
                        var images = new List<string>();
                        if (playerJson.TryGetProperty("images", out var imagesProperty))
                        {
                            foreach (var image in imagesProperty.EnumerateArray())
                            {
                                images.Add(image.GetString());
                            }
                        }

                        var playerEntity = new Player(connectionId, name)
                        {
                            ImagesProperty = images,
                            GameId = gameEntity.Id,
                            IsReady = true
                        };

                        gameEntity.PlayersCollection.Add(playerEntity);
                        playersCount++;
                    }
                }

                JsonElement roundsProperty;
                bool hasRounds = jsonData.TryGetProperty("rounds", out roundsProperty);

                if (hasRounds)
                {
                    foreach (var roundJson in roundsProperty.EnumerateArray())
                    {
                        var number = roundJson.GetProperty("number").GetInt32();
                        var duration = roundJson.GetProperty("duration").GetInt32();
                        var image = roundJson.GetProperty("image").GetString();
                        var correctAnswer = roundJson.GetProperty("correctAnswer").GetString();

                        var roundEntity = new Round
                        {
                            Number = number,
                            Duration = duration,
                            Image = image,
                            CorrectAnswer = correctAnswer,
                            GameId = gameEntity.Id
                        };

                        JsonElement answersProperty;
                        bool hasAnswers = roundJson.TryGetProperty("answers", out answersProperty);

                        if (hasAnswers)
                        {
                            foreach (var answerJson in answersProperty.EnumerateArray())
                            {
                                var player = answerJson.GetProperty("player").GetString();
                                var playersAnswer = answerJson.GetProperty("playersAnswer").GetString();
                                var timeRemaining = answerJson.GetProperty("timeRemaining").GetInt32();
                                var score = answerJson.GetProperty("score").GetInt32();

                                var answerEntity = new Answer
                                {
                                    Player = player,
                                    PlayersAnswer = playersAnswer,
                                    TimeRemaining = timeRemaining,
                                    Score = score
                                };

                                roundEntity.AnswersCollection.Add(answerEntity);
                            }
                        }

                        gameEntity.RoundsCollection.Add(roundEntity);
                        roundsCount++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Game imported successfully",
                    GameId = gameEntity.Id,
                    GameCode = gameEntity.Code,
                    PlayersCount = playersCount,
                    RoundsCount = roundsCount,
                    Debug = new 
                    {
                        AvailableProperties = properties,
                        HasPlayers = hasPlayers,
                        HasRounds = hasRounds
                    }
                });
            }
            catch (JsonException ex)
            {
                return BadRequest($"Invalid JSON format: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error importing game: {ex.Message}");
            }
        }
    }
}