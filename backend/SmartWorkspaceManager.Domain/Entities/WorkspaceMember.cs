using System;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class WorkspaceMember
    {
        // Composite key: (UserId, WorkspaceId) - configure in Persistence
        public Guid UserId { get; set; }
        public Guid WorkspaceId { get; set; }

        public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public Workspace? Workspace { get; set; }
    }
}
