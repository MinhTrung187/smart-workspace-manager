using System;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed class CreateColumnRequest
    {
        public Guid BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
    }

    public sealed class UpdateColumnRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
    }
    public sealed class MoveColumnRequest
    {
        public int NewIndex { get; set; }
    }
    public sealed record ColumnResponse(
        Guid Id,
        Guid BoardId,
        string Name,
        int Position,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
