using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.API.Hubs
{
    public class CommentHub : Hub
    {
        public async Task JoinTask(string taskId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"task_{taskId}");
        }

        public async Task LeaveTask(string taskId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task_{taskId}");
        }
    }
}
