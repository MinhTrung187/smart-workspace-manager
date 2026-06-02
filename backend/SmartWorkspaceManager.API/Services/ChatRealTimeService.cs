using Microsoft.AspNetCore.SignalR;
using SmartWorkspaceManager.API.Hubs;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.API.Services
{
    public sealed class ChatRealTimeService : IChatRealTimeService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatRealTimeService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        private string GetGroupKey(Guid channelId) => $"channel_{channelId}";

        public async Task NotifyMessageSentAsync(Guid channelId, ChatMessageResponse message)
        {
            await _hubContext.Clients.Group(GetGroupKey(channelId)).SendAsync("ReceiveChatMessage", message);
        }
    }
}
