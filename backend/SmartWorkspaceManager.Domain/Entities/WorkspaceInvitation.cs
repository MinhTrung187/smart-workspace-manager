using System;
using SmartWorkspaceManager.Domain.Common;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class WorkspaceInvitation : BaseEntity
    {
        public Guid WorkspaceId { get; set; }

        public string Email { get; set; } = string.Empty;

        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime ExpiredAt { get; set; }

        public Workspace? Workspace { get; set; }

        public WorkspaceInvitation()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
