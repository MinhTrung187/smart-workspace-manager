using System;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class WorkspaceMember
    {
        public Guid UserId { get; set; }
        public Guid WorkspaceId { get; set; }

        public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Workspace? Workspace { get; set; }
    }
}
