using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Services
{
    public class ColumnService : IColumnService
    {
        private readonly IGenericRepository<Column> _columnRepository;
        private readonly IGenericRepository<Board> _boardRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IUserContext _userContext;

        public ColumnService(
            IGenericRepository<Column> columnRepository,
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserContext userContext)
        {
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<ColumnResponse> CreateColumnAsync(CreateColumnRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Column name is required.");

            if (request.Name.Length > 200)
                throw new ArgumentException("Column name cannot exceed 200 characters.");

            var board = await _boardRepository.GetByIdAsync(request.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(
                w => w.Id == board.WorkspaceId,
                "Members"
            );

            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var column = new Column
            {
                BoardId = request.BoardId,
                Name = request.Name.Trim(),
                Position = request.Position
            };

            await _columnRepository.AddAsync(column);
            await _columnRepository.SaveChangesAsync();

            return new ColumnResponse(
                column.Id,
                column.BoardId,
                column.Name,
                column.Position,
                column.CreatedAt,
                column.UpdatedAt
            );
        }

        public async Task<List<ColumnResponse>> GetColumnsByBoardAsync(Guid boardId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var board = await _boardRepository.GetByIdAsync(boardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(
                w => w.Id == board.WorkspaceId,
                "Members"
            );

            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var columns = await _columnRepository.FindAsync(c => c.BoardId == boardId, Array.Empty<string>());
            return columns
               .OrderBy(c => c.Position)
               .Select(c => new ColumnResponse(c.Id, c.BoardId, c.Name, c.Position, c.CreatedAt, c.UpdatedAt))
               .ToList();
        }

        public async Task<ColumnResponse> GetColumnByIdAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var columns = await _columnRepository.FindAsync(c => c.Id == id, "Board");
            var column = columns.FirstOrDefault();
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = column.Board ?? await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            return new ColumnResponse(column.Id, column.BoardId, column.Name, column.Position, column.CreatedAt, column.UpdatedAt);
        }

        public async Task<ColumnResponse> UpdateColumnAsync(Guid id, UpdateColumnRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Column name is required.");

            if (request.Name.Length > 200)
                throw new ArgumentException("Column name cannot exceed 200 characters.");

            var columns = await _columnRepository.FindAsync(c => c.Id == id, "Board");
            var column = columns.FirstOrDefault();
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = column.Board ?? await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            // Permission: only workspace owner may update columns (adjust if you want looser rules)
            if (workspace.OwnerId != userId.Value)
                throw new UnauthorizedAccessException("Only the workspace owner can update columns.");

            column.Name = request.Name.Trim();
            column.Position = request.Position;
            column.Touch();
            _columnRepository.Update(column);
            await _columnRepository.SaveChangesAsync();

            return new ColumnResponse(column.Id, column.BoardId, column.Name, column.Position, column.CreatedAt, column.UpdatedAt);
        }

        public async Task DeleteColumnAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var columns = await _columnRepository.FindAsync(c => c.Id == id, "Board");
            var column = columns.FirstOrDefault();
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = column.Board ?? await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            // Permission: only workspace owner may delete columns (adjust as needed)
            if (workspace.OwnerId != userId.Value)
                throw new UnauthorizedAccessException("Only the workspace owner can delete columns.");

            column.SoftDelete();
            _columnRepository.Update(column);
            await _columnRepository.SaveChangesAsync();
        }
    }
}
