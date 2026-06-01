using Microsoft.AspNetCore.SignalR;

namespace SmartWorkspaceManager.API.Hubs;

public class BoardHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync(
            "ReceiveMessage",
            message
        );
    }
}