using System;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface ICommentRealTimeService
    {
        Task NotifyCommentAddedAsync(Guid taskId, TaskCommentDto comment);
        Task NotifyCommentDeletedAsync(Guid taskId, Guid commentId);
    }
}
