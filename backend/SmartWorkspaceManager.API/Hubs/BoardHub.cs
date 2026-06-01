using Microsoft.AspNetCore.SignalR;

namespace SmartWorkspaceManager.API.Hubs;

public class BoardHub : Hub
{
    public async Task JoinBoard(string boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board_{boardId}");
    }

    public async Task LeaveBoard(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board_{boardId}");
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync(
            "ReceiveMessage",
            message
        );
    }
}