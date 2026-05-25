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
    public class BoardTaskService : IBoardTaskService
    {
        private readonly IGenericRepository<BoardTask> _taskRepository;
        private readonly IGenericRepository<Column> _columnRepository;
        private readonly IGenericRepository<Board> _boardRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public BoardTaskService(
            IGenericRepository<BoardTask> taskRepository,
            IGenericRepository<Column> columnRepository,
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<BoardTaskResponse> CreateTaskAsync(CreateBoardTaskRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Task title is required.");

            if (request.Title.Length > 200)
                throw new ArgumentException("Task title cannot exceed 200 characters.");

            var column = await _columnRepository.GetByIdAsync(request.ColumnId);
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            // map priority string to enum if possible, fallback to Medium
            if (!Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
                priority = TaskPriority.Medium;

            var task = new BoardTask
            {
                ColumnId = request.ColumnId,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DueDate = request.DueDate,
                Priority = priority,
                Position = request.Position,
                CreatedBy = userId.Value
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            return new BoardTaskResponse(
                task.Id,
                task.ColumnId,
                task.Title,
                task.Description,
                task.DueDate,
                task.Priority.ToString(),
                task.Position,
                task.CreatedBy,
                task.CreatedAt,
                task.UpdatedAt
            );
        }

        public async Task<List<BoardTaskResponse>> GetTasksByColumnAsync(Guid columnId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var column = await _columnRepository.GetByIdAsync(columnId);
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var tasks = await _taskRepository.FindAsync(t => t.ColumnId == columnId, "Assignees");
            return tasks
                .OrderBy(t => t.Position)
                .Select(t => new BoardTaskResponse(
                    t.Id,
                    t.ColumnId,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Priority.ToString(),
                    t.Position,
                    t.CreatedBy,
                    t.CreatedAt,
                    t.UpdatedAt
                ))
                .ToList();
        }

        public async Task<BoardTaskResponse> GetTaskByIdAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");
            
            var tasks = await _taskRepository.FindAsync(t => t.Id == id, "Column");
            var task = tasks.FirstOrDefault();
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var column = task.Column;
            if (column == null)
                throw new KeyNotFoundException("Column relation is missing or not included.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId); 
            if (board == null)
                throw new KeyNotFoundException("Board not found for this task.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            return new BoardTaskResponse(
                task.Id,
                task.ColumnId,
                task.Title,
                task.Description,
                task.DueDate,
                task.Priority.ToString(),
                task.Position,
                task.CreatedBy,
                task.CreatedAt,
                task.UpdatedAt
            );
        }

        public async Task<BoardTaskResponse> UpdateTaskAsync(Guid id, UpdateBoardTaskRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Task title is required.");

            if (request.Title.Length > 200)
                throw new ArgumentException("Task title cannot exceed 200 characters.");

            var tasks = await _taskRepository.FindAsync(t => t.Id == id, "Column");
            var task = tasks.FirstOrDefault();
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var column = task.Column ?? await _columnRepository.GetByIdAsync(task.ColumnId);
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == userId.Value || task.CreatedBy == userId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You do not have permission to update this task.");

            if (request.ColumnId.HasValue && request.ColumnId.Value != task.ColumnId)
            {
                var targetColumn = await _columnRepository.GetByIdAsync(request.ColumnId.Value);
                if (targetColumn == null)
                    throw new KeyNotFoundException("Target column not found.");

                var targetBoard = await _boardRepository.GetByIdAsync(targetColumn.BoardId);
                if (targetBoard == null || targetBoard.WorkspaceId != board.WorkspaceId)
                    throw new InvalidOperationException("Target column must belong to the same workspace.");
                task.ColumnId = request.ColumnId.Value;
            }

            task.Title = request.Title.Trim();
            task.Description = request.Description?.Trim();
            task.DueDate = request.DueDate;
            if (!Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
                priority = TaskPriority.Medium;
            task.Priority = priority;
            task.Position = request.Position;

            task.Touch();
            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync();

            return new BoardTaskResponse(
                task.Id,
                task.ColumnId,
                task.Title,
                task.Description,
                task.DueDate,
                task.Priority.ToString(),
                task.Position,
                task.CreatedBy,
                task.CreatedAt,
                task.UpdatedAt
            );
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var tasks = await _taskRepository.FindAsync(t => t.Id == id, "Column");
            var task = tasks.FirstOrDefault();
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var column = task.Column ?? await _columnRepository.GetByIdAsync(task.ColumnId);
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == userId.Value || task.CreatedBy == userId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You do not have permission to delete this task.");

            task.SoftDelete();
            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync();
        }
    }
}
