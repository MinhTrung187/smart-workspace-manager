using System;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class TaskAssignee
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }

        public BoardTask? Task { get; set; }
        public User? User { get; set; }
    }
}
