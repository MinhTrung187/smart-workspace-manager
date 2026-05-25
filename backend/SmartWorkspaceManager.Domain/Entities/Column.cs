using System;
using SmartWorkspaceManager.Domain.Common;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class Column : BaseEntity
    {
        public Guid BoardId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Position { get; set; }

        public Board? Board { get; set; }

        public Column()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
