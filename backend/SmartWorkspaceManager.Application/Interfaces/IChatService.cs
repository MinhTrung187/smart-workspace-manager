using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatMessageResponse>> GetMessagesAsync(Guid channelId);
        Task<ChatMessageResponse> SendMessageAsync(Guid channelId, CreateChatMessageRequest request);
    }
}
