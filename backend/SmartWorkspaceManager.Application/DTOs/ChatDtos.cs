using System;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed class CreateChatMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    public sealed record ChatMessageResponse(
        Guid Id,
        Guid ChannelId,
        Guid UserId,
        string UserName,
        string Content,
        DateTime CreatedAt,
        DateTime? EditedAt,
        bool SentByMe
    );
}
