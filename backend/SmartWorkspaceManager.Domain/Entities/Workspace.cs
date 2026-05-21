using System;
using System.Collections.Generic;
using SmartWorkspaceManager.Domain.Common;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }

        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    }
}
