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
        string Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
