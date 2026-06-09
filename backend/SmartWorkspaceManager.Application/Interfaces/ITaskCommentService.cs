using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface ITaskCommentService
    {
        Task<TaskCommentDto> AddCommentAsync(Guid taskId, string content);
        Task<IReadOnlyList<TaskCommentDto>> GetCommentsByTaskAsync(Guid taskId);
        Task DeleteCommentAsync(Guid id);
    }
}
