using System;
using SmartWorkspaceManager.Domain.Common;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class TaskComment : BaseEntity
    {
        public Guid TaskId { get; set; }

        public Guid UserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public BoardTask? Task { get; set; }
        public User? User { get; set; }

        public TaskComment()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
