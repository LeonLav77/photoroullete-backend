using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vjezba.Model;

namespace Vjezba.Web;

public interface ILobbyManager
{
    Task CreateLobby(JObject parsed, string connectionId, IHubCallerClients clients);
    Task JoinLobby(JObject parsed, string connectionId, IHubCallerClients clients, IGroupManager groups);
    Task RequestLobbyState(JObject parsed, IHubCallerClients clients);
    Lobby GetLobby(string lobbyCode);
    void RemovePlayerFromLobby(string lobbyCode, string connectionId);
    void RemoveLobby(string lobbyCode);
    List<string> GetLobbyCodesForPlayer(string connectionId);
}

public class LobbyManager : ILobbyManager
{
    private static readonly Dictionary<string, Lobby> _lobbies = new Dictionary<string, Lobby>();

    public async Task CreateLobby(JObject parsed, string connectionId, IHubCallerClients clients)
    {
        Random random = new Random();
        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string lobbyCode = new string(Enumerable.Repeat(letters, 5)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        Lobby lobby = new Lobby
        {
            Code = lobbyCode,
            Players = new List<Player>()
        };
        
        _lobbies[lobbyCode] = lobby;

        await clients.Caller.SendAsync("LobbyCreated", lobbyCode);
    }

    public async Task JoinLobby(JObject parsed, string connectionId, IHubCallerClients clients, IGroupManager groups)
    {
        string lobbyCode = parsed["data"]["lobbyCode"]?.ToString();
        
        if (!_lobbies.ContainsKey(lobbyCode))
        {
            await clients.Caller.SendAsync("LobbyNotFound", lobbyCode);
            return;
        }

        Lobby lobby = _lobbies[lobbyCode];

        if (lobby.Players.Count >= 4)
        {
            await clients.Caller.SendAsync("LobbyFull", lobbyCode);
            return;
        }

        var connectionManager = new ConnectionManager();
        
        if (!connectionManager.HasName(connectionId))
        {
            await clients.Caller.SendAsync("Error", "Please set your name first");
            return;
        }

        if (!connectionManager.HasImages(connectionId))
        {
            await clients.Caller.SendAsync("Error", "Please prepare images first");
            return;
        }

        // Create a new Player object
        Player player = new Player(connectionId, connectionManager.GetName(connectionId))
        {
            Images = new List<string>(connectionManager.GetImages(connectionId))
        };

        // Add player to lobby
        lobby.Players.Add(player);
        _lobbies[lobbyCode] = lobby;

        await groups.AddToGroupAsync(connectionId, lobbyCode);

        await clients.Caller.SendAsync("LobbyJoined", lobbyCode);

        await clients.Group(lobbyCode).SendAsync("dd", connectionId);

        var lobbyState = new
        {
            code = lobby.Code,
            players = lobby.Players.Select(p => p.ConnectionId).ToList(),
            playerNames = lobby.Players.Select(p => new { id = p.ConnectionId, name = p.Name }).ToList(),
            playerImages = lobby.Players.ToDictionary(p => p.ConnectionId, p => p.Images)
        };

        string lobbyStateJson = JsonConvert.SerializeObject(lobbyState);

        await clients.Group(lobbyCode).SendAsync("LobbyState", lobbyStateJson);
    }

    public async Task RequestLobbyState(JObject parsed, IHubCallerClients clients)
    {
        string requestedLobbyCode = parsed["data"]["lobbyCode"]?.ToString();
        
        if (_lobbies.ContainsKey(requestedLobbyCode))
        {
            var lobby = _lobbies[requestedLobbyCode];
            var lobbyState = new
            {
                code = lobby.Code,
                players = lobby.Players.Select(p => p.ConnectionId).ToList(),
                playerNames = lobby.Players.Select(p => new { id = p.ConnectionId, name = p.Name }).ToList(),
                playerImages = lobby.Players.ToDictionary(p => p.ConnectionId, p => p.Images)
            };

            string lobbyStateJson = JsonConvert.SerializeObject(lobbyState);

            await clients.Caller.SendAsync("LobbyState", lobbyStateJson);
        }
    }

    public Lobby GetLobby(string lobbyCode)
    {
        return _lobbies.TryGetValue(lobbyCode, out var lobby) ? lobby : null;
    }

    public void RemovePlayerFromLobby(string lobbyCode, string connectionId)
    {
        if (_lobbies.ContainsKey(lobbyCode))
        {
            var lobby = _lobbies[lobbyCode];
            var player = lobby.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player != null)
            {
                lobby.Players.Remove(player);
            }
        }
    }

    public void RemoveLobby(string lobbyCode)
    {
        _lobbies.Remove(lobbyCode);
    }

    public List<string> GetLobbyCodesForPlayer(string connectionId)
    {
        return _lobbies.Where(kvp => kvp.Value.Players.Any(p => p.ConnectionId == connectionId))
                      .Select(kvp => kvp.Key)
                      .ToList();
    }
}