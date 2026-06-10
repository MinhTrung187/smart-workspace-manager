    using System;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.Common;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IActivityLogService
    {
        Task LogAsync(ActivityType type, Guid workspaceId, Guid? taskId = null, string? description = null);
        Task<PagedList<ActivityLogDto>> GetWorkspaceLogsAsync(Guid workspaceId, int page = 1, int pageSize = 50);
    }
}
