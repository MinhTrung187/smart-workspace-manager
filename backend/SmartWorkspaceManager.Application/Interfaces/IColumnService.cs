using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IColumnService
    {
        Task<ColumnResponse> CreateColumnAsync(CreateColumnRequest request);
        Task<List<ColumnResponse>> GetColumnsByBoardAsync(Guid boardId);
        Task<ColumnResponse> GetColumnByIdAsync(Guid id);
        Task<ColumnResponse> UpdateColumnAsync(Guid id, UpdateColumnRequest request);
        Task DeleteColumnAsync(Guid id);
        Task<ColumnResponse> MoveColumnAsync(Guid id, MoveColumnRequest request);

    }
}
