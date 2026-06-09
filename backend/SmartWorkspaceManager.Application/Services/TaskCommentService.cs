using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly IGenericRepository<TaskComment> _commentRepository;
        private readonly IGenericRepository<BoardTask> _taskRepository;
        private readonly IGenericRepository<Column> _columnRepository;
        private readonly IGenericRepository<Board> _boardRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;
        private readonly ICommentRealTimeService _commentRealTimeService;

        public TaskCommentService(
            IGenericRepository<TaskComment> commentRepository,
            IGenericRepository<BoardTask> taskRepository,
            IGenericRepository<Column> columnRepository,
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserRepository userRepository,
            IUserContext userContext,
            ICommentRealTimeService commentRealTimeService)
        {
            _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _commentRealTimeService = commentRealTimeService ?? throw new ArgumentNullException(nameof(commentRealTimeService));
        }

        public async Task<TaskCommentDto> AddCommentAsync(Guid taskId, string content)
        {
            var actorId = _userContext.UserId;
            if (actorId == null || actorId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated.");

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Comment content is required.");

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

            var comment = new TaskComment
            {
                TaskId = taskId,
                UserId = actorId.Value,
                Content = content.Trim()
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            var user = await _userRepository.GetByIdAsync(actorId.Value);
            var userName = user?.FullName ?? string.Empty;

            var result = new TaskCommentDto(
                comment.Id,
                comment.TaskId,
                comment.UserId,
                userName,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt
            );

            await _commentRealTimeService.NotifyCommentAddedAsync(taskId, result);

            return result;
        }

        public async Task<IReadOnlyList<TaskCommentDto>> GetCommentsByTaskAsync(Guid taskId)
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

            var comments = (await _commentRepository.FindAsync(c => c.TaskId == taskId, "User"))
                .OrderBy(c => c.CreatedAt)
                .ToList();

            // preload user names for efficiency
            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var userDict = new Dictionary<Guid, Domain.Entities.User>();
            foreach (var uid in userIds)
            {
                var u = await _userRepository.GetByIdAsync(uid);
                if (u != null) userDict[uid] = u;
            }

            var result = comments.Select(c => new TaskCommentDto(
                c.Id,
                c.TaskId,
                c.UserId,
                userDict.TryGetValue(c.UserId, out var uu) ? uu.FullName : string.Empty,
                c.Content,
                c.CreatedAt,
                c.UpdatedAt
            )).ToList();

            return result;
        }

        public async Task DeleteCommentAsync(Guid id)
        {
            var actorId = _userContext.UserId;
            if (actorId == null || actorId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated.");

            var comments = await _commentRepository.FindAsync(c => c.Id == id, "Task");
            var comment = comments.FirstOrDefault();
            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            var task = comment.Task ?? await _taskRepository.GetByIdAsync(comment.TaskId);
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

            // permission: only workspace owner or comment author may delete
            var isAllowed = workspace.OwnerId == actorId.Value || comment.UserId == actorId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("Only the workspace owner or the comment author can delete this comment.");

            // soft delete
            comment.SoftDelete();
            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();

            await _commentRealTimeService.NotifyCommentDeletedAsync(comment.TaskId, id);
        }
    }
}
    