using System;
using System.Collections.Generic;

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

    public sealed class BoardQueryOptions
    {
        public int TasksPage { get; set; } = 1;
        public int TasksPageSize { get; set; } = 50;

        public string? TaskSearch { get; set; }

        public string? TaskSortBy { get; set; }
        public bool TaskSortDesc { get; set; } = false;
    }

    public sealed record BoardColumnDetailDto(
        Guid Id,
        Guid BoardId,
        string Name,
        int Position,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<BoardTaskDto> Tasks
    );

    public sealed record BoardDetailResponse(
        Guid Id,
        Guid WorkspaceId,
        string WorkspaceName,
        string Name,
        string CreatedByName,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<BoardColumnDetailDto> Columns
    );
}
