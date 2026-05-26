using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface ITaskAssigneeService
    {
        Task<TaskAssigneeResponse> AssignAsync(Guid taskId, Guid userId);
        Task RemoveAssignmentAsync(Guid taskId, Guid userId);
        Task<List<TaskAssigneeResponse>> GetAssigneesByTaskAsync(Guid taskId);
        Task<List<BoardTaskResponse>> GetTasksByUserAsync(Guid userId);
        Task<List<BoardTaskResponse>> GetMyTasksAsync();
    }
}
