using System;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed record TaskAttachmentDto(
        Guid Id,
        Guid TaskId,
        string FileName,
        string FileUrl,
        long FileSize,
        string? ContentType,
        Guid UploadedById,
        string UploadedByName,
        DateTime CreatedAt
    );
    public sealed record FileUploadResult(
        string FileUrl,
        string FileName,
        long FileSize,
        string? ContentType
);
}