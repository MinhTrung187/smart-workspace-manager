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
        private readonly IBoardRealTimeService _realTimeService;

        public ColumnService(
            IGenericRepository<Column> columnRepository,
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserContext userContext,
            IBoardRealTimeService realTimeService)
        {
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
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

            // Auto-increment position: start at 1000 for the first column, otherwise max+1000.
            var existingColumns = await _columnRepository.FindAsync(c => c.BoardId == request.BoardId, Array.Empty<string>());
            var maxPosition = existingColumns.Any()
            ? existingColumns.Max(c => c.Position)
            : 0;

            var newPosition = maxPosition + 1000;

            var column = new Column
            {
                BoardId = request.BoardId,
                Name = request.Name.Trim(),
                Position = newPosition
            };

            await _columnRepository.AddAsync(column);
            await _columnRepository.SaveChangesAsync();

            await _realTimeService.NotifyColumnCreatedAsync(column.BoardId, column.Id);

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


            column.Name = request.Name.Trim();
            column.Position = request.Position;
            column.Touch();
            _columnRepository.Update(column);
            await _columnRepository.SaveChangesAsync();

            await _realTimeService.NotifyColumnUpdatedAsync(column.BoardId, column.Id);

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

            var isAllowed = workspace.OwnerId == userId.Value || board.CreatedBy == userId.Value;

            if ( !isAllowed )
                throw new UnauthorizedAccessException(" You don't have permission to delete this column .");

            column.SoftDelete();
            _columnRepository.Update(column);
            await _columnRepository.SaveChangesAsync();

            await _realTimeService.NotifyColumnDeletedAsync(column.BoardId, column.Id);
        }
        public async Task<ColumnResponse> MoveColumnAsync(Guid id, MoveColumnRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var columnsFound = await _columnRepository.FindAsync(c => c.Id == id, "Board");
            var column = columnsFound.FirstOrDefault();
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = column.Board ?? await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");


            var allColumns = (await _columnRepository.FindAsync(c => c.BoardId == board.Id, Array.Empty<string>()))
                .OrderBy(c => c.Position)
                .ToList();

            var currentIndex = allColumns.FindIndex(c => c.Id == column.Id);
            if (currentIndex < 0)
                throw new KeyNotFoundException("Column not found in board.");

            var n = allColumns.Count;
            var targetIndex = request.NewIndex;
            if (targetIndex < 1) targetIndex = 1;
            if (targetIndex > n) targetIndex = n;

            if (targetIndex - 1 == currentIndex)
            {
                return new ColumnResponse(column.Id, column.BoardId, column.Name, column.Position, column.CreatedAt, column.UpdatedAt);
            }

            allColumns.RemoveAt(currentIndex);
            allColumns.Insert(targetIndex - 1, column);

            var position = 1000;
            foreach (var col in allColumns)
            {
                if (col.Position != position)
                {
                    col.Position = position;
                    col.Touch();
                    _columnRepository.Update(col);
                }
                position += 1000; 
            }

            await _columnRepository.SaveChangesAsync();

            await _realTimeService.NotifyColumnMovedAsync(column.BoardId, column.Id);

            var moved = allColumns.First(c => c.Id == column.Id);
            return new ColumnResponse(moved.Id, moved.BoardId, moved.Name, moved.Position, moved.CreatedAt, moved.UpdatedAt);
        }
    }
}

