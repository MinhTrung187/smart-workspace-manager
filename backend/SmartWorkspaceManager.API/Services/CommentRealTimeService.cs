using Microsoft.AspNetCore.SignalR;
using SmartWorkspaceManager.API.Hubs;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.API.Services
{
    public sealed class CommentRealTimeService : ICommentRealTimeService
    {
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentRealTimeService(IHubContext<CommentHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        private string GetGroupKey(Guid taskId) => $"task_{taskId}";

        public async Task NotifyCommentAddedAsync(Guid taskId, TaskCommentDto comment)
        {
            await _hubContext.Clients.Group(GetGroupKey(taskId)).SendAsync("CommentAdded", comment);
        }

        public async Task NotifyCommentDeletedAsync(Guid taskId, Guid commentId)
        {
            await _hubContext.Clients.Group(GetGroupKey(taskId)).SendAsync("CommentDeleted", commentId);
        }
    }
}
