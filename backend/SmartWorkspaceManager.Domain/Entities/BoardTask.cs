using System;
using System.Collections.Generic;
using SmartWorkspaceManager.Domain.Common;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class BoardTask : BaseEntity
    {
        public Guid ColumnId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public int Position { get; set; }

        public Guid CreatedBy { get; set; }

        public Column? Column { get; set; }
        public User? Creator { get; set; }

        public ICollection<TaskAssignee> Assignees { get; set; } = new List<TaskAssignee>();

        public BoardTask()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
