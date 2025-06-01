using Vjezba.Model;

namespace Vjezba.Web.Services
{
    public class GameValidationService
    {
        public ValidationResult ValidateGameId(int id)
        {
            if (id <= 0)
            {
                return ValidationResult.Error("Invalid game ID");
            }
            return ValidationResult.Success();
        }

        public ValidationResult ValidateGameData(Game game, int requestedId)
        {
            if (game == null)
            {
                return ValidationResult.Error($"Game with ID {requestedId} not found", 404);
            }

            if (game.PlayersCollection == null)
            {
                return ValidationResult.Error("Game data is corrupted - no players collection");
            }

            if (game.RoundsCollection == null)
            {
                return ValidationResult.Error("Game data is corrupted - no rounds collection");
            }

            return ValidationResult.Success();
        }

        public bool IsValidPlayer(Player player)
        {
            return !string.IsNullOrWhiteSpace(player?.ConnectionId);
        }

        public ValidationResult ValidateScore(int score)
        {
            if (score > 5000)
            {
                return ValidationResult.Error("Score cannot be higher than 5000");
            }
            
            return ValidationResult.Success();
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Error(string message, int statusCode = 400)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = message, 
                StatusCode = statusCode 
            };
        }
    }
}