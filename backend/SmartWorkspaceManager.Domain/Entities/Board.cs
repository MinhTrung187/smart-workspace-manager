using System;
using SmartWorkspaceManager.Domain.Common;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class Board : BaseEntity
    {
        public Guid WorkspaceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }

        public Workspace? Workspace { get; set; }
        public User? Creator { get; set; }

        public Board()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
