    using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IBoardService
    {
        Task<BoardResponse> CreateBoardAsync(CreateBoardRequest request);
        Task<List<BoardResponse>> GetBoardsByWorkspaceAsync(Guid workspaceId);
        Task<BoardResponse> GetBoardByIdAsync(Guid id);
        Task<BoardResponse> UpdateBoardAsync(Guid id, UpdateBoardRequest request);
        Task DeleteBoardAsync(Guid id);
    }
}
