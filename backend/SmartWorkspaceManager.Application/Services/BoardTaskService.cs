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
        private readonly IBoardRealTimeService _realTimeService;

        public BoardTaskService(
            IGenericRepository<BoardTask> taskRepository,
            IGenericRepository<Column> columnRepository,
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserRepository userRepository,
            IUserContext userContext,
            IBoardRealTimeService realTimeService)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
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

            if (!Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
                priority = TaskPriority.Medium;

            var existingTasks = await _taskRepository.FindAsync(t => t.ColumnId == request.ColumnId, Array.Empty<string>());
            var maxPosition = existingTasks.Any()
                ? existingTasks.Max(t => t.Position)
                : 0;

            var newPosition = maxPosition + 1000;
            var task = new BoardTask
            {
                ColumnId = request.ColumnId,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DueDate = request.DueDate,
                Priority = priority,
                Position = newPosition,
                CreatedBy = userId.Value
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            await _realTimeService.NotifyTaskCreatedAsync(column.BoardId, task.Id);

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

            await _realTimeService.NotifyTaskUpdatedAsync(column.BoardId, task.Id);

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

            await _realTimeService.NotifyTaskDeletedAsync(column.BoardId, task.Id);
        }
        public async Task<BoardTaskResponse> MoveTaskAsync(Guid id, MoveTaskRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var tasksFound = await _taskRepository.FindAsync(t => t.Id == id, "Column");
            var task = tasksFound.FirstOrDefault();
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var sourceColumn = task.Column ?? await _columnRepository.GetByIdAsync(task.ColumnId);
            if (sourceColumn == null)
                throw new KeyNotFoundException("Source column not found.");

            var sourceBoard = await _boardRepository.GetByIdAsync(sourceColumn.BoardId);
            if (sourceBoard == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == sourceBoard.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");


            var targetColumn = await _columnRepository.GetByIdAsync(request.TargetColumnId);
            if (targetColumn == null)
                throw new KeyNotFoundException("Target column not found.");

            var targetBoard = await _boardRepository.GetByIdAsync(targetColumn.BoardId);
            if (targetBoard == null || targetBoard.WorkspaceId != sourceBoard.WorkspaceId)
                throw new InvalidOperationException("Target column must belong to the same workspace.");

            if (targetColumn.Id == sourceColumn.Id)
            {
                var tasksInColumn = (await _taskRepository.FindAsync(t => t.ColumnId == sourceColumn.Id, Array.Empty<string>()))
                    .OrderBy(t => t.Position)
                    .ToList();

                var currentIndex = tasksInColumn.FindIndex(t => t.Id == task.Id);
                if (currentIndex < 0) throw new KeyNotFoundException("Task not found in source column.");

                var n = tasksInColumn.Count;
                var targetIndex = request.NewIndex;
                if (targetIndex < 1) targetIndex = 1;
                if (targetIndex > n) targetIndex = n;

                if (targetIndex - 1 == currentIndex)
                {
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

                tasksInColumn.RemoveAt(currentIndex);
                tasksInColumn.Insert(targetIndex - 1, task);

                var pos = 1000;

                foreach (var t in tasksInColumn)
                {
                    if (t.Position != pos)
                    {
                        t.Position = pos;
                        t.Touch();
                        _taskRepository.Update(t);
                    }

                    pos += 1000;
                }

                await _taskRepository.SaveChangesAsync();

                await _realTimeService.NotifyTaskMovedAsync(sourceColumn.BoardId, task.Id);

                var moved = tasksInColumn.First(t => t.Id == task.Id);
                return new BoardTaskResponse(
                    moved.Id,
                    moved.ColumnId,
                    moved.Title,
                    moved.Description,
                    moved.DueDate,
                    moved.Priority.ToString(),
                    moved.Position,
                    moved.CreatedBy,
                    moved.CreatedAt,
                    moved.UpdatedAt
                );
            }


            var sourceTasks = (await _taskRepository.FindAsync(t => t.ColumnId == sourceColumn.Id, Array.Empty<string>()))
                .OrderBy(t => t.Position)
                .ToList();

            var targetTasks = (await _taskRepository.FindAsync(t => t.ColumnId == targetColumn.Id, Array.Empty<string>()))
                .OrderBy(t => t.Position)
                .ToList();

            sourceTasks.RemoveAll(t => t.Id == task.Id);

            var m = targetTasks.Count;
            var targetIndex2 = request.NewIndex;
            if (targetIndex2 < 1) targetIndex2 = 1;
            if (targetIndex2 > m + 1) targetIndex2 = m + 1;


            task.ColumnId = targetColumn.Id;
            targetTasks.Insert(targetIndex2 - 1, task);

            var posS = 1000;
            foreach (var t in sourceTasks)
            {
                if (t.Position != posS)
                {
                    t.Position = posS;
                    t.Touch();
                    _taskRepository.Update(t);
                }
                posS+=1000;
            }

            var posT = 1000;
            foreach (var t in targetTasks)
            {
                if (t.Position != posT || t.ColumnId != targetColumn.Id)
                {
                    t.Position = posT;
                    t.ColumnId = targetColumn.Id;
                    t.Touch();
                    _taskRepository.Update(t);
                }
                posT+=1000;
            }

            await _taskRepository.SaveChangesAsync();

            await _realTimeService.NotifyTaskMovedAsync(sourceColumn.BoardId, task.Id);

            var movedTask = targetTasks.First(t => t.Id == task.Id);
            return new BoardTaskResponse(
                movedTask.Id,
                movedTask.ColumnId,
                movedTask.Title,
                movedTask.Description,
                movedTask.DueDate,
                movedTask.Priority.ToString(),
                movedTask.Position,
                movedTask.CreatedBy,
                movedTask.CreatedAt,
                movedTask.UpdatedAt
            );
        }
    }
}