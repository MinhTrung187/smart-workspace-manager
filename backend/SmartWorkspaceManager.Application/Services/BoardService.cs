using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Application.Services
{
    public class BoardService : IBoardService
    {
        private readonly IGenericRepository<Board> _boardRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IGenericRepository<Column> _columnRepository;
        private readonly IGenericRepository<BoardTask> _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public BoardService(
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IGenericRepository<Column> columnRepository,
            IGenericRepository<BoardTask> taskRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<BoardResponse> CreateBoardAsync(CreateBoardRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Board name is required.");

            if (request.Name.Length > 200)
                throw new ArgumentException("Board name cannot exceed 200 characters.");

            // Verify workspace exists and current user is member or owner
            var workspaces = await _workspaceRepository.FindAsync(
                w => w.Id == request.WorkspaceId,
                "Members"
            );

            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var board = new Board
            {
                WorkspaceId = request.WorkspaceId,
                Name = request.Name.Trim(),
                CreatedBy = userId.Value
            };

            await _boardRepository.AddAsync(board);
            await _boardRepository.SaveChangesAsync();

            return new BoardResponse(
                board.Id,
                board.WorkspaceId,
                board.Name,
                board.CreatedBy,
                board.CreatedAt,
                board.UpdatedAt
            );
        }

        public async Task<List<BoardResponse>> GetBoardsByWorkspaceAsync(Guid workspaceId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var workspaces = await _workspaceRepository.FindAsync(
                w => w.Id == workspaceId,
                "Members"
            );

            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var boards = await _boardRepository.FindAsync(
                b => b.WorkspaceId == workspaceId,
                "Creator"
            );

            return boards.Select(b => new BoardResponse(
                b.Id,
                b.WorkspaceId,
                b.Name,
                b.CreatedBy,
                b.CreatedAt,
                b.UpdatedAt
            )).ToList();
        }

        public async Task<BoardDetailResponse> GetBoardByIdAsync(Guid id, BoardQueryOptions? options = null)
        {
            options ??= new BoardQueryOptions();

            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var boards = await _boardRepository.FindAsync(
                b => b.Id == id,
                "Workspace", "Creator"
            );

            var board = boards.FirstOrDefault();
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of the workspace for this board.");

            var boardCreatorName = board.Creator?.FullName ?? (await _userRepository.GetByIdAsync(board.CreatedBy))?.FullName ?? string.Empty;

            var columns = (await _columnRepository.FindAsync(c => c.BoardId == board.Id, Array.Empty<string>()))
                .OrderBy(c => c.Position)
                .ToList();

            var columnIds = columns.Select(c => c.Id).ToHashSet();
            var tasks = columnIds.Count == 0
                ? new List<BoardTask>()
                : (await _taskRepository.FindAsync(t => columnIds.Contains(t.ColumnId), "Creator"))
                    .ToList();

            tasks = ApplyTaskSearch(tasks, options.TaskSearch);
            tasks = ApplyTaskSorting(tasks, options.TaskSortBy, options.TaskSortDesc);
            tasks = ApplyTaskPaging(tasks, options.TasksPage, options.TasksPageSize);

            var tasksByColumn = tasks
                .GroupBy(t => t.ColumnId)
                .ToDictionary(g => g.Key, g => g.Select(ToBoardTaskDto).ToList());

            var columnDtos = columns.Select(c => new BoardColumnDetailDto(
                c.Id,
                c.BoardId,
                c.Name,
                c.Position,
                c.CreatedAt,
                c.UpdatedAt,
                tasksByColumn.TryGetValue(c.Id, out var columnTasks) ? columnTasks : new List<BoardTaskDto>()
            )).ToList();

            return new BoardDetailResponse(
                board.Id,
                board.WorkspaceId,
                workspace.Name,
                board.Name,
                boardCreatorName,
                board.CreatedAt,
                board.UpdatedAt,
                columnDtos
            );
        }

        public async Task<BoardResponse> UpdateBoardAsync(Guid id, UpdateBoardRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Board name is required.");

            if (request.Name.Length > 200)
                throw new ArgumentException("Board name cannot exceed 200 characters.");

            var boards = await _boardRepository.FindAsync(
                b => b.Id == id,
                "Workspace"
            );

            var board = boards.FirstOrDefault();
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            // permission: allow workspace owner or board creator
            var workspace = board.Workspace;
            if (workspace == null)
            {
                var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
                workspace = workspaces.FirstOrDefault();
            }

            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == userId.Value || board.CreatedBy == userId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You don't have permission to update this board.");

            board.Name = request.Name.Trim();
            board.Touch();
            _boardRepository.Update(board);
            await _boardRepository.SaveChangesAsync();

            return new BoardResponse(
                board.Id,
                board.WorkspaceId,
                board.Name,
                board.CreatedBy,
                board.CreatedAt,
                board.UpdatedAt
            );
        }

        public async Task DeleteBoardAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var boards = await _boardRepository.FindAsync(
                b => b.Id == id,
                "Workspace"
            );

            var board = boards.FirstOrDefault();
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspace = board.Workspace;
            if (workspace == null)
            {
                var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
                workspace = workspaces.FirstOrDefault();
            }

            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == userId.Value || board.CreatedBy == userId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You don't have permission to delete this board.");

            // soft-delete
            board.SoftDelete();
            _boardRepository.Update(board);
            await _boardRepository.SaveChangesAsync();
        }

        private static List<BoardTask> ApplyTaskSearch(List<BoardTask> tasks, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return tasks;

            var normalizedSearch = search.Trim();
            return tasks
                .Where(t =>
                    t.Title.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        private static List<BoardTask> ApplyTaskSorting(List<BoardTask> tasks, string? sortBy, bool sortDesc)
        {
            var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

            IOrderedEnumerable<BoardTask> orderedTasks = normalizedSortBy switch
            {
                "title" => sortDesc
                    ? tasks.OrderByDescending(t => t.Title)
                    : tasks.OrderBy(t => t.Title),
                "duedate" or "due_date" => sortDesc
                    ? tasks.OrderByDescending(t => t.DueDate.HasValue).ThenByDescending(t => t.DueDate)
                    : tasks.OrderByDescending(t => t.DueDate.HasValue).ThenBy(t => t.DueDate),
                "priority" => sortDesc
                    ? tasks.OrderByDescending(t => GetPriorityRank(t.Priority))
                    : tasks.OrderBy(t => GetPriorityRank(t.Priority)),
                "createdat" or "created_at" => sortDesc
                    ? tasks.OrderByDescending(t => t.CreatedAt)
                    : tasks.OrderBy(t => t.CreatedAt),
                "updatedat" or "updated_at" => sortDesc
                    ? tasks.OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                    : tasks.OrderBy(t => t.UpdatedAt ?? t.CreatedAt),
                _ => sortDesc
                    ? tasks.OrderByDescending(t => t.Position)
                    : tasks.OrderBy(t => t.Position)
            };

            return orderedTasks.ThenBy(t => t.CreatedAt).ToList();
        }

        private static List<BoardTask> ApplyTaskPaging(List<BoardTask> tasks, int page, int pageSize)
        {
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Max(1, pageSize);

            return tasks
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToList();
        }

        private static BoardTaskDto ToBoardTaskDto(BoardTask task)
        {
            return new BoardTaskDto(
                task.Id,
                task.ColumnId,
                task.Title,
                task.Description,
                task.DueDate,
                task.Priority.ToString(),
                task.Position,
                task.Creator?.FullName ?? string.Empty,
                task.CreatedAt,
                task.UpdatedAt
            );
        }

        private static int GetPriorityRank(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => 1,
                TaskPriority.Medium => 2,
                TaskPriority.High => 3,
                _ => 0
            };
        }
    }
}
