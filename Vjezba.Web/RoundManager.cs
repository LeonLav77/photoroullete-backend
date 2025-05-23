using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Vjezba.Model;

namespace Vjezba.Web;

public interface IRoundManager
{
    Task StartRound(string lobbyCode, IHubCallerClients clients, Game game, int roundNumber = 1);
    void SavePlayerAnswer(Round round, string connectionId, string answer, int timeRemaining);
    Dictionary<string, int> GetLeaderboard(Game game);
}

public class RoundManager : IRoundManager
{
    private static readonly int ROUNDS = 5;
    private static readonly int ROUND_TIME = 5;

    public async Task StartRound(string lobbyCode, IHubCallerClients clients, Game game, int roundNumber = 1)
    {
        if (IsGameComplete(roundNumber))
        {
            await EndGame(lobbyCode, clients, game);
            return;
        }

        var newRound = CreateNewRound(game, roundNumber);
        game.Rounds.Add(newRound);

        await SendRoundToClients(lobbyCode, clients, newRound);
        await StartRoundTimer(lobbyCode, clients, game, roundNumber);
    }

    public void SavePlayerAnswer(Round round, string connectionId, string answer, int timeRemaining)
    {
        var existingAnswer = GetExistingAnswer(round, connectionId);
        
        if (existingAnswer == null)
        {
            var answerObj = CreateAnswerObject(connectionId, answer, timeRemaining);
            round.Answers.Add(answerObj);
            Console.WriteLine($"Answer saved for player {connectionId}: {answer} with {timeRemaining}ms remaining");
        }
        else
        {
            Console.WriteLine($"Player {connectionId} already has an answer for this round");
        }
    }

    public Dictionary<string, int> GetLeaderboard(Game game)
    {
        var leaderboard = new Dictionary<string, int>();

        foreach (var round in game.Rounds)
        {
            if (round.Answers == null) continue;

            AccumulateScoresFromRound(leaderboard, round);
        }

        return SortLeaderboard(leaderboard);
    }

    private bool IsGameComplete(int roundNumber)
    {
        return roundNumber > ROUNDS;
    }

    private async Task EndGame(string lobbyCode, IHubCallerClients clients, Game game)
    {
        var leaderboard = GetLeaderboard(game);
        var jsonLeaderboard = JsonConvert.SerializeObject(leaderboard);
        
        Console.WriteLine("Game over");
        await clients.Group(lobbyCode).SendAsync("GameOver", jsonLeaderboard);
    }

    private Round CreateNewRound(Game game, int roundNumber)
    {
        var imageEntry = game.Images.ElementAt(roundNumber - 1);
        
        return new Round
        {
            Number = roundNumber,
            Image = imageEntry.Key,
            Duration = ROUND_TIME,
            CorrectAnswer = imageEntry.Value,
            Answers = new List<Answer>()
        };
    }

    private async Task SendRoundToClients(string lobbyCode, IHubCallerClients clients, Round round)
    {
        string roundJson = JsonConvert.SerializeObject(round);
        await clients.Group(lobbyCode).SendAsync("RoundStarted", roundJson);
    }

    private async Task StartRoundTimer(string lobbyCode, IHubCallerClients clients, Game game, int roundNumber)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(ROUND_TIME * 1000);
                await ProcessRoundEnd(lobbyCode, clients, game, roundNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in timer: {ex.Message}");
            }
        });
    }

    private async Task ProcessRoundEnd(string lobbyCode, IHubCallerClients clients, Game game, int roundNumber)
    {
        var currentRound = GetCurrentRound(game);
        if (currentRound == null) return;

        await SendCorrectAnswer(lobbyCode, clients, game, currentRound);
        await Task.Delay(500);

        ProcessAllPlayerAnswers(game, currentRound);
        var leaderboard = GetLeaderboard(game);
        
        await SendRoundResults(lobbyCode, clients, leaderboard);
        await Task.Delay(2200);
        
        await StartRound(lobbyCode, clients, game, roundNumber + 1);
    }

    private async Task SendCorrectAnswer(string lobbyCode, IHubCallerClients clients, Game game, Round currentRound)
    {
        var correctAnswerPlayer = game.Players.FirstOrDefault(p => p.ConnectionId == currentRound.CorrectAnswer);
        var json = JsonConvert.SerializeObject(correctAnswerPlayer);
        await clients.Group(lobbyCode).SendAsync("CorrectAnswer", json);
    }

    private void ProcessAllPlayerAnswers(Game game, Round currentRound)
    {
        foreach (var player in game.Players)
        {
            var existingAnswer = GetExistingAnswer(currentRound, player.ConnectionId);

            if (existingAnswer != null)
            {
                ScoreExistingAnswer(existingAnswer, currentRound, player.ConnectionId);
            }
            else
            {
                CreateDefaultAnswer(currentRound, player.ConnectionId);
            }
        }
    }

    private void ScoreExistingAnswer(Answer answer, Round round, string playerId)
    {
        var points = CalculateScore(answer, round);
        answer.Score = points;
        Console.WriteLine($"Player {playerId} scored {points} points");
    }

    private void CreateDefaultAnswer(Round round, string playerId)
    {
        var defaultAnswer = new Answer
        {
            Player = playerId,
            PlayersAnswer = "No answer",
            TimeRemaining = 0,
            Score = 0
        };
        round.Answers.Add(defaultAnswer);
        Console.WriteLine($"Player {playerId} didn't answer, scored 0 points");
    }

    private async Task SendRoundResults(string lobbyCode, IHubCallerClients clients, Dictionary<string, int> leaderboard)
    {
        var jsonLeaderboard = JsonConvert.SerializeObject(leaderboard);
        await clients.Group(lobbyCode).SendAsync("RoundEnded", jsonLeaderboard);
    }

    private int CalculateScore(Answer answer, Round round)
    {
        if (!IsAnswerCorrect(answer, round))
        {
            return 0;
        }

        return CalculateSpeedBonus(answer.TimeRemaining);
    }

    private bool IsAnswerCorrect(Answer answer, Round round)
    {
        return round.CorrectAnswer == answer.PlayersAnswer;
    }

    private int CalculateSpeedBonus(int timeRemaining)
    {
        var maxPoints = ROUND_TIME * 1000;
        var timeTaken = maxPoints - timeRemaining;
        var points = Math.Max(100, maxPoints - timeTaken);

        Console.WriteLine($"Correct answer! Time remaining: {timeRemaining}ms, Points awarded: {points}");
        return points;
    }

    private Answer GetExistingAnswer(Round round, string connectionId)
    {
        return round.Answers?.FirstOrDefault(a => a.Player == connectionId);
    }

    private Answer CreateAnswerObject(string connectionId, string answer, int timeRemaining)
    {
        return new Answer
        {
            Player = connectionId,
            PlayersAnswer = answer,
            TimeRemaining = timeRemaining
        };
    }

    private void AccumulateScoresFromRound(Dictionary<string, int> leaderboard, Round round)
    {
        foreach (var answer in round.Answers)
        {
            if (string.IsNullOrEmpty(answer.Player)) continue;

            if (!leaderboard.ContainsKey(answer.Player))
            {
                leaderboard[answer.Player] = 0;
            }

            leaderboard[answer.Player] += answer.Score;
        }
    }

    private Dictionary<string, int> SortLeaderboard(Dictionary<string, int> leaderboard)
    {
        return leaderboard.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    private Round GetCurrentRound(Game game)
    {
        return game.Rounds.LastOrDefault();
    }
}