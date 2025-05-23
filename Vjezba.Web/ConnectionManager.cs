using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace Vjezba.Web;

public interface IConnectionManager
{
    Task SetName(JObject parsed, string connectionId);
    Task SetPreparedImages(JObject parsed, string connectionId);
    Task HandleDisconnection(string connectionId, IHubCallerClients clients, IGroupManager groups, ILobbyManager lobbyManager);
    bool HasName(string connectionId);
    bool HasImages(string connectionId);
    string GetName(string connectionId);
    List<string> GetImages(string connectionId);
}

public class ConnectionManager : IConnectionManager
{
    private static readonly Dictionary<string, string> _clientNames = new Dictionary<string, string>();
    private static readonly Dictionary<string, List<string>> _clientImages = new Dictionary<string, List<string>>();

    public async Task SetName(JObject parsed, string connectionId)
    {
        string name = parsed["data"]["name"]?.ToString();
        _clientNames[connectionId] = name;
    }

    public async Task SetPreparedImages(JObject parsed, string connectionId)
    {
        List<string> images = parsed["data"]["images"]?.ToObject<List<string>>();
        _clientImages[connectionId] = images;
    }

    public bool HasName(string connectionId)
    {
        return _clientNames.ContainsKey(connectionId);
    }

    public bool HasImages(string connectionId)
    {
        return _clientImages.ContainsKey(connectionId) && _clientImages[connectionId].Count > 0;
    }

    public string GetName(string connectionId)
    {
        return _clientNames.TryGetValue(connectionId, out var name) ? name : null;
    }

    public List<string> GetImages(string connectionId)
    {
        return _clientImages.TryGetValue(connectionId, out var images) ? images : new List<string>();
    }

    public async Task HandleDisconnection(string connectionId, IHubCallerClients clients, IGroupManager groups, ILobbyManager lobbyManager)
    {
        // Clean up any lobbies the user was in
        var lobbyCodesWithPlayer = lobbyManager.GetLobbyCodesForPlayer(connectionId);
        
        foreach (var lobbyCode in lobbyCodesWithPlayer)
        {
            var lobby = lobbyManager.GetLobby(lobbyCode);
            if (lobby != null)
            {
                lobbyManager.RemovePlayerFromLobby(lobbyCode, connectionId);
                
                if (lobby.Players.Count == 0)
                {
                    lobbyManager.RemoveLobby(lobbyCode);
                }
                else
                {
                    await clients.Group(lobbyCode).SendAsync("PlayerLeft", connectionId);
                }
                
                await groups.RemoveFromGroupAsync(connectionId, lobbyCode);
            }
        }
        
        // Clean up dictionaries
        _clientNames.Remove(connectionId);
        _clientImages.Remove(connectionId);
    }
}