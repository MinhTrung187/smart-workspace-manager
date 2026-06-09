using System;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed record CreateTaskCommentRequest(
        string Content
    );

    public sealed record TaskCommentDto(
        Guid Id,
        Guid TaskId,
        Guid UserId,
        string UserName,
        string? AvatarUrl,
        string Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
