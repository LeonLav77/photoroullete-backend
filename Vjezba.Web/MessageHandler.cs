using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace Vjezba.Web;

public interface IMessageHandler
{
    Task HandleMessage(JObject parsed, HubCallerContext context, IHubCallerClients clients, IGroupManager groups);
}

public class MessageHandler : IMessageHandler
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILobbyManager _lobbyManager;
    private readonly IGameManager _gameManager;

    public MessageHandler(IConnectionManager connectionManager, ILobbyManager lobbyManager, IGameManager gameManager)
    {
        _connectionManager = connectionManager;
        _lobbyManager = lobbyManager;
        _gameManager = gameManager;
    }

    public async Task HandleMessage(JObject parsed, HubCallerContext context, IHubCallerClients clients, IGroupManager groups)
    {
        string type = parsed["type"]?.ToString();

        switch (type)
        {
            case "test":
                await clients.All.SendAsync("dd", type);
                break;
            case "setName":
                await _connectionManager.SetName(parsed, context.ConnectionId);
                break;
            case "preparedImages":
                await _connectionManager.SetPreparedImages(parsed, context.ConnectionId);
                break;
            case "createLobby":
                await _lobbyManager.CreateLobby(parsed, context.ConnectionId, clients);
                break;
            case "joinLobby":
                await _lobbyManager.JoinLobby(parsed, context.ConnectionId, clients, groups);
                break;
            case "requestLobbyState":
                await _lobbyManager.RequestLobbyState(parsed, clients);
                break;
            case "startGame":
                await _gameManager.PrepareGame(parsed, clients);
                break;
            case "turnOverImages":
                await _gameManager.TurnOverImages(parsed, context.ConnectionId, clients, groups);
                break;
            case "submitAnswer":
                await _gameManager.SaveAnswer(parsed, context.ConnectionId);
                break;
            case "playerReady":
                await _gameManager.HandlePlayerReady(parsed, context.ConnectionId, _lobbyManager, clients);
                break;
        }
    }
}