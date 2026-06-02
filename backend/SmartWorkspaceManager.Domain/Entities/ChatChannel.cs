using System;
using System.Collections.Generic;
using SmartWorkspaceManager.Domain.Common;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class ChatChannel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ChannelType Type { get; set; } = ChannelType.Workspace;

        public Guid WorkspaceId { get; set; }

        public Guid? TaskId { get; set; }

        public Workspace? Workspace { get; set; }

        public BoardTask? Task { get; set; }

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public ChatChannel()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
