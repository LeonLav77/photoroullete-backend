using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vjezba.Model;
using System.Collections.Concurrent;

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
    // Use ConcurrentDictionary for thread safety in cloud environments
    private static readonly ConcurrentDictionary<string, Game> _games = new ConcurrentDictionary<string, Game>();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _playerBase64Images = 
        new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
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

        Console.WriteLine($"[{startLobbyCode}] Image requests sent to players");
    }

    public async Task TurnOverImages(JObject parsed, string connectionId, IHubCallerClients clients, IGroupManager groups)
    {
        var base64Images = parsed["data"]["images"]?.ToObject<List<string>>();
        string lobbyCode = ExtractLobbyCode(parsed);

        Console.WriteLine($"[{lobbyCode}] Received {base64Images?.Count ?? 0} base64 images from {connectionId}");

        if (base64Images == null || !base64Images.Any())
        {
            Console.WriteLine($"[{lobbyCode}] No images received from {connectionId}");
            return;
        }

        StorePlayerImages(lobbyCode, base64Images, connectionId);
        
        var currentImageCount = _playerBase64Images.ContainsKey(lobbyCode) ? _playerBase64Images[lobbyCode].Count : 0;
        Console.WriteLine($"[{lobbyCode}] Total images stored: {currentImageCount}/{ROUNDS}");

        if (AllImagesReceived(lobbyCode))
        {
            Console.WriteLine($"[{lobbyCode}] All images received, starting game");
            await StartGame(lobbyCode, clients, groups);
        }
        else
        {
            // Notify other players about progress
            await clients.Group(lobbyCode).SendAsync("ImageUploadProgress", new { 
                received = currentImageCount, 
                total = ROUNDS 
            });
        }
    }

    public async Task SaveAnswer(JObject parsed, string connectionId)
    {
        string answer = parsed["data"]["answer"]?.ToString();
        string lobbyCode = ExtractLobbyCode(parsed);
        int timeRemaining = (int)parsed["data"]["timeRemaining"];

        if (!_games.TryGetValue(lobbyCode, out var game)) 
        {
            Console.WriteLine($"[{lobbyCode}] Game not found for saving answer");
            return;
        }

        var currentRound = game.Rounds.LastOrDefault();

        if (currentRound != null)
        {
            _roundManager.SavePlayerAnswer(currentRound, connectionId, answer, timeRemaining);
            Console.WriteLine($"[{lobbyCode}] Answer saved for {connectionId}");
        }
    }

    public async Task HandlePlayerReady(JObject parsed, string connectionId, ILobbyManager lobbyManager, IHubCallerClients clients)
    {
        string lobbyCode = ExtractLobbyCode(parsed);
        var lobby = GetLobbyForCode(lobbyCode);

        if (lobby == null) 
        {
            Console.WriteLine($"[{lobbyCode}] Lobby not found for player ready");
            return;
        }

        if (SetPlayerReady(lobby, connectionId) && lobby.AllPlayersReady())
        {
            Console.WriteLine($"[{lobbyCode}] All players are ready");
            
            if (_games.TryGetValue(lobbyCode, out var game))
            {
                await _roundManager.StartRound(lobbyCode, clients, game, 1);
            }
            else
            {
                Console.WriteLine($"[{lobbyCode}] Game not found when trying to start round");
            }
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
            Console.WriteLine($"Sent image request to {image.Key}: {image.Value.Count} images");
        }
    }

    private void StorePlayerImages(string lobbyCode, List<string> base64Images, string connectionId)
    {
        // Initialize the lobby's image dictionary if it doesn't exist
        var lobbyImages = _playerBase64Images.GetOrAdd(lobbyCode, _ => new ConcurrentDictionary<string, string>());

        foreach (var base64Image in base64Images)
        {
            // Use TryAdd to avoid overwriting existing images
            if (lobbyImages.TryAdd(base64Image, connectionId))
            {
                Console.WriteLine($"[{lobbyCode}] Stored image from {connectionId}");
            }
            else
            {
                Console.WriteLine($"[{lobbyCode}] Image already exists from {connectionId}");
            }
        }
    }

    private bool AllImagesReceived(string lobbyCode)
    {
        if (!_playerBase64Images.TryGetValue(lobbyCode, out var lobbyImages))
        {
            Console.WriteLine($"[{lobbyCode}] No images found for lobby");
            return false;
        }
        
        bool allReceived = lobbyImages.Count >= ROUNDS;
        Console.WriteLine($"[{lobbyCode}] Image check: {lobbyImages.Count}/{ROUNDS} - All received: {allReceived}");
        return allReceived;
    }

    private async Task StartGame(string lobbyCode, IHubCallerClients clients, IGroupManager groups)
    {
        var lobby = GetLobbyForCode(lobbyCode);
        
        if (!_playerBase64Images.TryGetValue(lobbyCode, out var images))
        {
            Console.WriteLine($"[{lobbyCode}] No images found when starting game");
            return;
        }

        Game game = new Game
        {
            Code = lobbyCode,
            Players = lobby.Players,
            Images = images.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) // Convert to regular Dictionary
        };

        _games.TryAdd(lobbyCode, game);
        await clients.Group(lobbyCode).SendAsync("GameStarted", lobbyCode);
        
        Console.WriteLine($"[{lobbyCode}] Game started successfully");
    }

    private bool SetPlayerReady(Lobby lobby, string connectionId)
    {
        var player = lobby.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
        
        if (player != null && !player.IsReady)
        {
            player.IsReady = true;
            Console.WriteLine($"Player {connectionId} is now ready in lobby {lobby.Code}");
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