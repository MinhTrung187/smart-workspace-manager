using System;
using SmartWorkspaceManager.Domain.Common;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class ActivityLog : BaseEntity
    {
        public Guid WorkspaceId { get; set; }

        public Guid UserId { get; set; }

        public ActivityType ActivityType { get; set; }

        public Guid? TaskId { get; set; }

        public string Description { get; set; } = string.Empty;

        public Workspace? Workspace { get; set; }
        public User? User { get; set; }
        public BoardTask? Task { get; set; }

        public ActivityLog()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
