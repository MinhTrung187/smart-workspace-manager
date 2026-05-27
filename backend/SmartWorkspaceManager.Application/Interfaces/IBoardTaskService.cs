using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IBoardTaskService
    {
        Task<BoardTaskResponse> CreateTaskAsync(CreateBoardTaskRequest request);
        Task<List<BoardTaskResponse>> GetTasksByColumnAsync(Guid columnId);
        Task<BoardTaskResponse> GetTaskByIdAsync(Guid id);
        Task<BoardTaskResponse> UpdateTaskAsync(Guid id, UpdateBoardTaskRequest request);
        Task DeleteTaskAsync(Guid id);
        Task<BoardTaskResponse> MoveTaskAsync(Guid id, MoveTaskRequest request);

    }
}
