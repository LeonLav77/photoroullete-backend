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
    
    private readonly IRoundManager _roundManager;

    public GameManager() : this(new RoundManager())
    {
    }

    public GameManager(IRoundManager roundManager)
    {
        _roundManager = roundManager;
    }

    public async Task PrepareGame(JObject parsed, IHubCallerClients clients)
    {
        string startLobbyCode = ExtractLobbyCode(parsed);
        var lobby = GetLobbyForCode(startLobbyCode);

        if (lobby == null) return;

        var randomizedImages = GetRandomizedImages(lobby);
        await SendImageRequestsToPlayers(randomizedImages, clients);

        Console.WriteLine("Image requests sent to players");
    }

    public async Task TurnOverImages(JObject parsed, string connectionId, IHubCallerClients clients, IGroupManager groups)
    {
        var base64Images = parsed["data"]["images"]?.ToObject<List<string>>();
        string lobbyCode = ExtractLobbyCode(parsed);

        Console.WriteLine($"Received base64 images: {base64Images.Count}");

        StorePlayerImages(lobbyCode, base64Images, connectionId);
        Console.WriteLine($"Total images stored: {_playerBase64Images[lobbyCode].Count}");

        if (AllImagesReceived(lobbyCode))
        {
            await StartGame(lobbyCode, clients, groups);
        }
    }

    public async Task SaveAnswer(JObject parsed, string connectionId)
    {
        string answer = parsed["data"]["answer"]?.ToString();
        string lobbyCode = ExtractLobbyCode(parsed);
        int timeRemaining = (int)parsed["data"]["timeRemaining"];

        if (!_games.ContainsKey(lobbyCode)) return;

        var game = _games[lobbyCode];
        var currentRound = game.Rounds.LastOrDefault();

        if (currentRound != null)
        {
            _roundManager.SavePlayerAnswer(currentRound, connectionId, answer, timeRemaining);
        }
    }

    public async Task HandlePlayerReady(JObject parsed, string connectionId, ILobbyManager lobbyManager, IHubCallerClients clients)
    {
        string lobbyCode = ExtractLobbyCode(parsed);
        var lobby = GetLobbyForCode(lobbyCode);

        if (lobby == null) return;

        if (SetPlayerReady(lobby, connectionId) && lobby.AllPlayersReady())
        {
            Console.WriteLine("All players are ready");
            await _roundManager.StartRound(lobbyCode, clients, _games[lobbyCode], 1);
        }
    }

    private string ExtractLobbyCode(JObject parsed)
    {
        return parsed["data"]["lobbyCode"]?.ToString();
    }

    private Lobby GetLobbyForCode(string lobbyCode)
    {
        var lobbyManager = new LobbyManager();
        return lobbyManager.GetLobby(lobbyCode);
    }

    private async Task SendImageRequestsToPlayers(Dictionary<string, string> randomizedImages, IHubCallerClients clients)
    {
        var groupedImages = randomizedImages.GroupBy(x => x.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Key).ToList());

        foreach (var image in groupedImages)
        {
            string requestedImages = JsonConvert.SerializeObject(image.Value);
            await clients.Client(image.Key).SendAsync("RequestImages", requestedImages);
        }
    }

    private void StorePlayerImages(string lobbyCode, List<string> base64Images, string connectionId)
    {
        if (!_playerBase64Images.ContainsKey(lobbyCode))
        {
            _playerBase64Images[lobbyCode] = new Dictionary<string, string>();
        }

        foreach (var base64Image in base64Images)
        {
            if (!_playerBase64Images[lobbyCode].ContainsKey(base64Image))
            {
                _playerBase64Images[lobbyCode][base64Image] = connectionId;
            }
        }
    }

    private bool AllImagesReceived(string lobbyCode)
    {
        return _playerBase64Images[lobbyCode].Count == ROUNDS;
    }

    private async Task StartGame(string lobbyCode, IHubCallerClients clients, IGroupManager groups)
    {
        var lobby = GetLobbyForCode(lobbyCode);
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

    private bool SetPlayerReady(Lobby lobby, string connectionId)
    {
        var player = lobby.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
        
        if (player != null && !player.IsReady)
        {
            player.IsReady = true;
            return true;
        }
        
        return false;
    }

    private Dictionary<string, string> GetRandomizedImages(Lobby lobby)
    {
        var playerImages = new Dictionary<string, string>();
        var random = new Random();

        for (int i = 0; i < ROUNDS; i++)
        {
            var randomPlayer = GetRandomPlayer(lobby, random);
            var randomImage = GetRandomImage(randomPlayer.Images, random);

            while (playerImages.ContainsKey(randomImage))
            {
                randomPlayer = GetRandomPlayer(lobby, random);
                randomImage = GetRandomImage(randomPlayer.Images, random);
            }

            playerImages.Add(randomImage, randomPlayer.ConnectionId);
        }

        return playerImages;
    }

    private Player GetRandomPlayer(Lobby lobby, Random random)
    {
        var randomIndex = random.Next(0, lobby.Players.Count);
        return lobby.Players[randomIndex];
    }

    private string GetRandomImage(List<string> images, Random random)
    {
        var randomIndex = random.Next(0, images.Count);
        return images[randomIndex];
    }
}