using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Vjezba.Model;

namespace Vjezba.Web;

public sealed class RouletteHub : Hub
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILobbyManager _lobbyManager;
    private readonly IGameManager _gameManager;
    private readonly IMessageHandler _messageHandler;

    public RouletteHub()
    {
        _connectionManager = new ConnectionManager();
        _lobbyManager = new LobbyManager();
        _gameManager = new GameManager();
        _messageHandler = new MessageHandler(_connectionManager, _lobbyManager, _gameManager);
    }

    public override async Task OnConnectedAsync()
    {
        await this.Clients.All.SendAsync("UserConnected", Context.ConnectionId);
    }

    public async Task SendMessage(string message)
    {
        var parsed = JObject.Parse(message);
        await _messageHandler.HandleMessage(parsed, Context, Clients, Groups);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await _connectionManager.HandleDisconnection(Context.ConnectionId, Clients, Groups, _lobbyManager);
        await base.OnDisconnectedAsync(exception);
    }
}