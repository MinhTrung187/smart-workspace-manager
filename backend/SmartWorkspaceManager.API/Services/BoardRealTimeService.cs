using Microsoft.AspNetCore.SignalR;
using SmartWorkspaceManager.API.Hubs;
using SmartWorkspaceManager.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.API.Services
{
    public sealed class BoardRealTimeService : IBoardRealTimeService
    {
        private readonly IHubContext<BoardHub> _hubContext;

        public BoardRealTimeService(IHubContext<BoardHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        private string GetGroupKey(Guid boardId) => $"board_{boardId}";

        public async Task NotifyBoardUpdatedAsync(Guid boardId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("BoardUpdated");
        }

        public async Task NotifyColumnCreatedAsync(Guid boardId, Guid columnId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("ColumnCreated", columnId);
        }

        public async Task NotifyColumnUpdatedAsync(Guid boardId, Guid columnId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("ColumnUpdated", columnId);
        }

        public async Task NotifyColumnDeletedAsync(Guid boardId, Guid columnId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("ColumnDeleted", columnId);
        }

        public async Task NotifyColumnMovedAsync(Guid boardId, Guid columnId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("ColumnMoved", columnId);
        }

        public async Task NotifyTaskCreatedAsync(Guid boardId, Guid taskId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("TaskCreated", taskId);
        }

        public async Task NotifyTaskUpdatedAsync(Guid boardId, Guid taskId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("TaskUpdated", taskId);
        }

        public async Task NotifyTaskDeletedAsync(Guid boardId, Guid taskId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("TaskDeleted", taskId);
        }

        public async Task NotifyTaskMovedAsync(Guid boardId, Guid taskId)
        {
            await _hubContext.Clients.Group(GetGroupKey(boardId)).SendAsync("TaskMoved", taskId);
        }
    }
}
