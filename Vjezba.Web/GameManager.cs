using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vjezba.Model;

namespace Vjezba.Web;

public interface IGameManager
{
    Task PrepareGame(JObject parsed, IHubCallerClients clients);
    Task TurnOverImages(JObject parsed, string connectionId, IHubCallerClients clients, IGroupManager groups);
    Task SaveAnswer(JObject parsed, string connectionId);
    Task HandlePlayerReady(JObject parsed, string connectionId, ILobbyManager lobbyManager, IHubCallerClients clients);
}

public class GameManager : IGameManager
{
    private static readonly Dictionary<string, Game> _games = new Dictionary<string, Game>();
    private static readonly Dictionary<string, Dictionary<string, string>> _playerBase64Images = new Dictionary<string, Dictionary<string, string>>();
    private static readonly int ROUNDS = 5;
    private static readonly int ROUND_TIME = 5;

    public async Task PrepareGame(JObject parsed, IHubCallerClients clients)
    {
        string startLobbyCode = parsed["data"]["lobbyCode"]?.ToString();
        var lobbyManager = new LobbyManager();
        var lobby = lobbyManager.GetLobby(startLobbyCode);

        if (lobby == null) return;

        Dictionary<string, string> randomizedImages = GetRandomizedImages(lobby);

        var groupedImages = randomizedImages.GroupBy(x => x.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Key).ToList());

        foreach (var image in groupedImages)
        {
            string requestedImages = JsonConvert.SerializeObject(image.Value);
            await clients.Client(image.Key).SendAsync("RequestImages", requestedImages);
        }

        Console.WriteLine("Image requests sent to players");
    }

    public async Task TurnOverImages(JObject parsed, string connectionId, IHubCallerClients clients, IGroupManager groups)
    {
        List<string> base64Images = parsed["data"]["images"]?.ToObject<List<string>>();
        string turnOverLobbyCode = parsed["data"]["lobbyCode"]?.ToString();

        Console.WriteLine($"Received base64 images: {base64Images.Count}");

        if (!_playerBase64Images.ContainsKey(turnOverLobbyCode))
        {
            _playerBase64Images[turnOverLobbyCode] = new Dictionary<string, string>();
        }

        foreach (var base64Image in base64Images)
        {
            if (!_playerBase64Images[turnOverLobbyCode].ContainsKey(base64Image))
            {
                _playerBase64Images[turnOverLobbyCode][base64Image] = connectionId;
            }
        }

        Console.WriteLine($"Received base64 images: {_playerBase64Images[turnOverLobbyCode].Count}");

        if (_playerBase64Images[turnOverLobbyCode].Count == ROUNDS)
        {
            await StartGame(turnOverLobbyCode, clients, groups);
        }
    }

    public async Task SaveAnswer(JObject parsed, string connectionId)
    {
        string answer = parsed["data"]["answer"]?.ToString();
        string lobbyCodeForAnswer = parsed["data"]["lobbyCode"]?.ToString();
        int timeRemaining = (int)parsed["data"]["timeRemaining"];

        if (_games.ContainsKey(lobbyCodeForAnswer))
        {
            var game = _games[lobbyCodeForAnswer];
            var round = game.Rounds.LastOrDefault();

            if (round != null)
            {
                // Check if player already has an answer for this round
                var existingAnswer = round.Answers?.FirstOrDefault(a => a.Player == connectionId);
                
                if (existingAnswer == null)
                {
                    var answerObj = new Answer
                    {
                        Player = connectionId,
                        PlayersAnswer = answer,
                        TimeRemaining = timeRemaining
                    };
                    round.Answers.Add(answerObj);
                    Console.WriteLine($"Answer saved for player {connectionId}: {answer} with {timeRemaining}ms remaining");
                }
                else
                {
                    Console.WriteLine($"Player {connectionId} already has an answer for this round");
                }
            }
        }
    }

    public async Task HandlePlayerReady(JObject parsed, string connectionId, ILobbyManager lobbyManager, IHubCallerClients clients)
    {
        string readyLobbyCode = parsed["data"]["lobbyCode"].ToString();

        var lobby = lobbyManager.GetLobby(readyLobbyCode);
        if (lobby != null)
        {
            var player = lobby.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

            if (player != null && !player.IsReady)
            {
                player.IsReady = true;

                if (lobby.AllPlayersReady())
                {
                    Console.WriteLine("All players are ready");
                    await StartRound(readyLobbyCode, clients);
                }
            }
        }
    }

    private async Task StartGame(string lobbyCode, IHubCallerClients clients, IGroupManager groups)
    {
        var lobbyManager = new LobbyManager();
        var lobby = lobbyManager.GetLobby(lobbyCode);
        var images = _playerBase64Images[lobbyCode];

        Game game = new Game
        {
            Code = lobbyCode,
            Players = lobby.Players,
            Images = images
        };

        _games[lobbyCode] = game;

        await clients.Group(lobbyCode).SendAsync("GameStarted", lobbyCode);
    }

    private async Task StartRound(string lobbyCode, IHubCallerClients clients, int roundNumber = 1)
    {
        if (roundNumber > ROUNDS)
        {
            var leaderboard = GetLeaderboardWithAllRounds(lobbyCode);

            var jsonLeaderboard = JsonConvert.SerializeObject(leaderboard);
            Console.WriteLine("Game over");
            await clients.Group(lobbyCode).SendAsync("GameOver", jsonLeaderboard);
            return;
        }

        var gameState = _games[lobbyCode];

        var newRound = new Round
        {
            Number = roundNumber,
            Image = gameState.Images.ElementAt(roundNumber - 1).Key,
            Duration = ROUND_TIME,
            CorrectAnswer = gameState.Images.ElementAt(roundNumber - 1).Value,
            Answers = new List<Answer>()
        };

        string sendObjectJson = JsonConvert.SerializeObject(newRound);

        gameState.Rounds.Add(newRound);

        await clients.Group(lobbyCode).SendAsync("RoundStarted", sendObjectJson);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(ROUND_TIME * 1000);

                var currentRound = GetCurrentRound(lobbyCode);
                if (currentRound == null) return;

                var correctAnswerPlayer = gameState.Players.FirstOrDefault(p => p.ConnectionId == currentRound.CorrectAnswer);
                var json = JsonConvert.SerializeObject(correctAnswerPlayer);

                await clients.Group(lobbyCode).SendAsync("CorrectAnswer", json);

                await Task.Delay(500);

                // Ensure all players have answers (create default answers for those who didn't respond)
                foreach (var player in gameState.Players)
                {
                    var existingAnswer = currentRound.Answers?.FirstOrDefault(a => a.Player == player.ConnectionId);

                    if (existingAnswer != null)
                    {
                        // Calculate score for existing answer
                        var points = CalculateRoundResults(existingAnswer, currentRound);
                        existingAnswer.Score = points;
                        Console.WriteLine($"Player {player.ConnectionId} scored {points} points");
                    }
                    else
                    {
                        // Create default answer for players who didn't respond
                        var newAnswer = new Answer
                        {
                            Player = player.ConnectionId,
                            PlayersAnswer = "No answer",
                            TimeRemaining = 0,
                            Score = 0
                        };
                        currentRound.Answers.Add(newAnswer);
                        Console.WriteLine($"Player {player.ConnectionId} didn't answer, scored 0 points");
                    }
                }

                // Update the round in the game
                SetCurrentRound(lobbyCode, currentRound);

                var leaderboard = GetLeaderboardWithAllRounds(lobbyCode);

                var jsonLeaderboard = JsonConvert.SerializeObject(leaderboard);

                await clients.Group(lobbyCode).SendAsync("RoundEnded", jsonLeaderboard);

                await Task.Delay(2200);
                
                await StartRound(lobbyCode, clients, roundNumber + 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in timer: {ex.Message}");
            }
        });
    }

    private Dictionary<string, int> GetLeaderboardWithAllRounds(string lobbyCode)
    {
        if (!_games.ContainsKey(lobbyCode))
        {
            return new Dictionary<string, int>();
        }

        var game = _games[lobbyCode];
        var leaderboard = new Dictionary<string, int>();

        foreach (var round in game.Rounds)
        {
            if (round.Answers == null) continue;

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

        return leaderboard.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    private int CalculateRoundResults(Answer answer, Round round)
    {
        // Only award points for correct answers
        if (round.CorrectAnswer != answer.PlayersAnswer)
        {
            return 0;
        }

        // For correct answers, award points based on speed
        // Faster answers get more points
        var maxPoints = ROUND_TIME * 1000; // Maximum possible points
        var timeTaken = maxPoints - answer.TimeRemaining; // Time it took to answer
        var points = Math.Max(100, maxPoints - timeTaken); // Minimum 100 points, max is ROUND_TIME * 1000

        Console.WriteLine($"Correct answer! Time remaining: {answer.TimeRemaining}ms, Points awarded: {points}");
        return points;
    }

    private Dictionary<string, string> GetRandomizedImages(Lobby lobby)
    {
        var playerImages = new Dictionary<string, string>();
        var random = new Random();

        for (int i = 0; i < 5; i++)
        {
            var randomNumber = random.Next(0, lobby.Players.Count);
            var player = lobby.Players[randomNumber];

            var randomImage = GetRandomItemFromList(player.Images);

            while (playerImages.ContainsKey(randomImage))
            {
                randomNumber = random.Next(0, lobby.Players.Count);
                player = lobby.Players[randomNumber];
                randomImage = GetRandomItemFromList(player.Images);
            }

            playerImages.Add(randomImage, player.ConnectionId);
        }

        return playerImages;
    }

    private string GetRandomItemFromList(List<string> list)
    {
        var random = new Random();
        int randomIndex = random.Next(0, list.Count);
        return list[randomIndex];
    }

    private Round? GetCurrentRound(string lobbyCode)
    {
        if (_games.ContainsKey(lobbyCode))
        {
            var game = _games[lobbyCode];
            return game.Rounds.LastOrDefault();
        }

        return null;
    }
    
    private void SetCurrentRound(string lobbyCode, Round round)
    {
        if (_games.ContainsKey(lobbyCode))
        {
            var game = _games[lobbyCode];
            if (game.Rounds.Count > 0)
            {
                game.Rounds[game.Rounds.Count - 1] = round;
            }
        }
    }
}