using System;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed class CreateBoardTaskRequest
    {
        public Guid ColumnId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = "Medium";
        public int Position { get; set; }
    }

    public sealed class UpdateBoardTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = "Medium";
        public int Position { get; set; }
        public Guid? ColumnId { get; set; }
    }

    public sealed record BoardTaskResponse(
        Guid Id,
        Guid ColumnId,
        string Title,
        string? Description,
        DateTime? DueDate,
        string Priority,
        int Position,
        Guid CreatedBy,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
