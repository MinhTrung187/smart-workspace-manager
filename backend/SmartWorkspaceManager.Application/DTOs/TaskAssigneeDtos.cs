using System;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed class AssignTaskRequest
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
    }

    public sealed record TaskAssigneeResponse(
        Guid TaskId,
        Guid UserId,
        string FullName,
        string Email,
        string? AvatarUrl
    );
}
