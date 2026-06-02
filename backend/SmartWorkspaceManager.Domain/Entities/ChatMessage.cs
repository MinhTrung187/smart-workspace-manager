using System;
using SmartWorkspaceManager.Domain.Common;

namespace SmartWorkspaceManager.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid ChannelId { get; set; }

        public Guid UserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime? EditedAt { get; set; }

        public ChatChannel? Channel { get; set; }

        public User? User { get; set; }

        public ChatMessage()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
