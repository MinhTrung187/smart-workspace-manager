using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.Common;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Application.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IGenericRepository<ActivityLog> _activityLogRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public ActivityLogService(
            IGenericRepository<ActivityLog> activityLogRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _activityLogRepository = activityLogRepository ?? throw new ArgumentNullException(nameof(activityLogRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task LogAsync(ActivityType type, Guid workspaceId, Guid? taskId = null, string? description = null)
        {
            // Use current user if available; caller can also set description to include actor info if desired.
            var userId = _userContext.UserId ?? Guid.Empty;

            var log = new ActivityLog
            {
                WorkspaceId = workspaceId,
                UserId = userId,
                ActivityType = type,
                TaskId = taskId,
                Description = description ?? string.Empty
            };

            await _activityLogRepository.AddAsync(log);
            await _activityLogRepository.SaveChangesAsync();
        }

        public async Task<PagedList<ActivityLogDto>> GetWorkspaceLogsAsync(Guid workspaceId, int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var logs = (await _activityLogRepository.FindAsync(l => l.WorkspaceId == workspaceId, "User", "Task"))
                .OrderByDescending(l => l.CreatedAt)
                .ToList();

            var total = logs.Count;
            var items = logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // preload users
            var userIds = items.Select(i => i.UserId).Distinct().ToList();
            var users = new Dictionary<Guid, Domain.Entities.User>();
            foreach (var uid in userIds)
            {
                var u = await _userRepository.GetByIdAsync(uid);
                if (u != null) users[uid] = u;
            }

            var dtos = items.Select(i => new ActivityLogDto(
                i.Id,
                i.WorkspaceId,
                i.UserId,
                users.TryGetValue(i.UserId, out var uu) ? uu.FullName : string.Empty,
                i.ActivityType,
                i.TaskId,
                i.Description,
                i.CreatedAt
            )).ToList();

            var paged = PagedList<ActivityLogDto>.Create(dtos, total, page, pageSize);
            return paged;
        }
    }
}
