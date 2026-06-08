using System;
using SmartWorkspaceManager.Domain.Common;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class TaskAttachment : BaseEntity
    {
        public Guid TaskId { get; set; }

        public Guid UploadedBy { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string? ContentType { get; set; }

        public BoardTask? Task { get; set; }

        public TaskAttachment()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
