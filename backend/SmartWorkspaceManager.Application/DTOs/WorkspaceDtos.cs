using SmartWorkspaceManager.Application.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed class CreateWorkspaceRequest
    {
        [Required]
        [MaxLength(200, ErrorMessage = "Workspace name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Workspace description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
    }

    public sealed record WorkspaceResponse(
        Guid Id,
        string Name,
        string? Description,
        Guid OwnerId,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    public sealed record UserWorkspacesResponse(
        int TotalWorkspacesCount,
        List<UserWorkspaceItemResponse> Workspaces
    );

    public sealed record UserWorkspaceItemResponse(
        Guid Id,
        string Name,
        string? Description,
        Guid OwnerId,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int MemberCount
    );

    public sealed class WorkspaceQueryOptions
    {
        public int BoardsPage { get; set; } = 1;
        public int BoardsPageSize { get; set; } = 50;

        public int TasksPage { get; set; } = 1;
        public int TasksPageSize { get; set; } = 50;

        public string? TaskSearch { get; set; }

        public string? TaskSortBy { get; set; }
        public bool TaskSortDesc { get; set; } = false;
    }

    public sealed record BoardTaskDto(
        Guid Id,
        Guid ColumnId,
        string Title,
        string? Description,
        DateTime? DueDate,
        string Priority,
        int Position,
        string CreatedByName,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    public sealed record ColumnBasicDto(
        Guid Id,
        Guid BoardId,
        string Name,
        int Position
    );

    public sealed record BoardDto(
        Guid Id,
        Guid WorkspaceId,
        string Name,
        string CreatedByName,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<ColumnBasicDto> Columns
    );

    public sealed record WorkspaceDetailResponse(
        Guid Id,
        string Name,
        string? Description,
        string OwnerName,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<BoardDto> Boards
    );

    public sealed record WorkspaceMemberDto(
        Guid UserId,
        string FullName,
        string Email,
        string? AvatarUrl,
        string Role,
        DateTime JoinedAt
    );

    public sealed class InviteUserRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }


}
