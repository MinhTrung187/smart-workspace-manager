using System;
using SmartWorkspaceManager.Application.Common;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed record ActivityLogDto(
        Guid Id,
        Guid WorkspaceId,
        Guid UserId,
        string UserName,
        string? AvatarUrl,
        ActivityType ActivityType,
        Guid? TaskId,
        string Description,
        DateTime CreatedAt
    );
}
