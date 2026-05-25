using System;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed class CreateBoardRequest
    {
        public Guid WorkspaceId { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class UpdateBoardRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed record BoardResponse(
        Guid Id,
        Guid WorkspaceId,
        string Name,
        Guid CreatedBy,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
