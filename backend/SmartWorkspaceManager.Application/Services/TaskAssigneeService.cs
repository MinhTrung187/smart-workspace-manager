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
    public class TaskAssigneeService : ITaskAssigneeService
    {
        private readonly IGenericRepository<TaskAssignee> _taskAssigneeRepository;
        private readonly IGenericRepository<BoardTask> _taskRepository;
        private readonly IGenericRepository<Column> _columnRepository;
        private readonly IGenericRepository<Board> _boardRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public TaskAssigneeService(
            IGenericRepository<TaskAssignee> taskAssigneeRepository,
            IGenericRepository<BoardTask> taskRepository,
            IGenericRepository<Column> columnRepository,
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _taskAssigneeRepository = taskAssigneeRepository ?? throw new ArgumentNullException(nameof(taskAssigneeRepository));
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<TaskAssigneeResponse> AssignAsync(Guid taskId, Guid userId)
        {
            var actorId = _userContext.UserId;
            if (actorId == null || actorId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated.");

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var column = await _columnRepository.GetByIdAsync(task.ColumnId);
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == actorId.Value || task.CreatedBy == actorId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You don't have permission to assign users to this task.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var isMember = workspace.Members.Any(m => m.UserId == userId);
            if (!isMember)
                throw new InvalidOperationException("User is not a member of the workspace.");

            var existing = (await _taskAssigneeRepository.FindAsync(a => a.TaskId == taskId && a.UserId == userId, Array.Empty<string>())).FirstOrDefault();
            if (existing != null)
                throw new InvalidOperationException("User is already assigned to this task.");

            var assignee = new TaskAssignee
            {
                TaskId = taskId,
                UserId = userId
            };

            await _taskAssigneeRepository.AddAsync(assignee);
            await _taskAssigneeRepository.SaveChangesAsync();

            return new TaskAssigneeResponse(
                assignee.TaskId,
                assignee.UserId,
                user.FullName ?? string.Empty,
                user.Email,
                user.AvatarUrl
            );
        }

        public async Task RemoveAssignmentAsync(Guid taskId, Guid userId)
        {
            var actorId = _userContext.UserId;
            if (actorId == null || actorId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated.");

            var assignments = await _taskAssigneeRepository.FindAsync(a => a.TaskId == taskId && a.UserId == userId, Array.Empty<string>());
            var assignment = assignments.FirstOrDefault();
            if (assignment == null)
                throw new KeyNotFoundException("Assignment not found.");

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var column = await _columnRepository.GetByIdAsync(task.ColumnId);
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == actorId.Value || task.CreatedBy == actorId.Value || assignment.UserId == actorId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You don't have permission to remove this assignment.");

            _taskAssigneeRepository.Delete(assignment);
            await _taskAssigneeRepository.SaveChangesAsync();
        }

        public async Task<List<TaskAssigneeResponse>> GetAssigneesByTaskAsync(Guid taskId)
        {
            var actorId = _userContext.UserId;
            if (actorId == null || actorId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated.");

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            var column = await _columnRepository.GetByIdAsync(task.ColumnId);
            if (column == null)
                throw new KeyNotFoundException("Column not found.");

            var board = await _boardRepository.GetByIdAsync(column.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == actorId.Value || workspace.Members.Any(m => m.UserId == actorId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var assignees = await _taskAssigneeRepository.FindAsync(a => a.TaskId == taskId, "User");
            return assignees.Select(a => new TaskAssigneeResponse(
                a.TaskId,
                a.UserId,
                a.User?.FullName ?? string.Empty,
                a.User?.Email ?? string.Empty,
                a.User?.AvatarUrl
            )).ToList();
        }

        public async Task<List<BoardTaskResponse>> GetTasksByUserAsync(Guid userId)
        {
            var actorId = _userContext.UserId;
            if (actorId == null || actorId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated.");

            if (actorId.Value != userId)
                throw new UnauthorizedAccessException("You can only fetch tasks for your own account.");

            var tasks = await _taskRepository.FindAsync(t => t.Assignees.Any(a => a.UserId == userId), Array.Empty<string>());
            return tasks.Select(t => new BoardTaskResponse(
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
            )).ToList();
        }

        public async Task<List<BoardTaskResponse>> GetMyTasksAsync()
        {
            var actorId = _userContext.UserId;
            if (actorId == null || actorId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated.");

            return await GetTasksByUserAsync(actorId.Value);
        }
    }
}
