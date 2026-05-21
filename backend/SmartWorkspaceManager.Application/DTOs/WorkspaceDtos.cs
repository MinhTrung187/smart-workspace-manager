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
}
