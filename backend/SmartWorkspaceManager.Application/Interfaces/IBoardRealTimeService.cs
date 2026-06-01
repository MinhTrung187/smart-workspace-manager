using System;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IBoardRealTimeService
    {
        Task NotifyBoardUpdatedAsync(Guid boardId);
        
        Task NotifyColumnCreatedAsync(Guid boardId, Guid columnId);
        Task NotifyColumnUpdatedAsync(Guid boardId, Guid columnId);
        Task NotifyColumnDeletedAsync(Guid boardId, Guid columnId);
        Task NotifyColumnMovedAsync(Guid boardId, Guid columnId);
        
        Task NotifyTaskCreatedAsync(Guid boardId, Guid taskId);
        Task NotifyTaskUpdatedAsync(Guid boardId, Guid taskId);
        Task NotifyTaskDeletedAsync(Guid boardId, Guid taskId);
        Task NotifyTaskMovedAsync(Guid boardId, Guid taskId);
    }
}
