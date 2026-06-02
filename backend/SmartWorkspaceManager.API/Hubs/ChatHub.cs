using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinChannel(string channelId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel_{channelId}");
        }

        public async Task LeaveChannel(string channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel_{channelId}");
        }
    }
}
