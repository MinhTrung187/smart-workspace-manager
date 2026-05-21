using System;
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

    public sealed record WorkspaceDetailResponse(
        Guid Id,
        string Name,
        string? Description,
        Guid OwnerId,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<WorkspaceMemberDto> Members
    );

    public sealed record WorkspaceMemberDto(
        Guid UserId,
        string FullName,
        string Email,
        string? AvatarUrl,
        string Role,
        DateTime JoinedAt
    );
}
