using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IGenericRepository<ChatChannel> _channelRepository;
        private readonly IGenericRepository<ChatMessage> _messageRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IGenericRepository<WorkspaceMember> _workspaceMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;
        private readonly IChatRealTimeService _chatRealTimeService;

        public ChatService(
            IGenericRepository<ChatChannel> channelRepository,
            IGenericRepository<ChatMessage> messageRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IGenericRepository<WorkspaceMember> workspaceMemberRepository,
            IUserRepository userRepository,
            IUserContext userContext,
            IChatRealTimeService chatRealTimeService)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _workspaceMemberRepository = workspaceMemberRepository ?? throw new ArgumentNullException(nameof(workspaceMemberRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _chatRealTimeService = chatRealTimeService ?? throw new ArgumentNullException(nameof(chatRealTimeService));
        }

        public async Task<List<ChatMessageResponse>> GetMessagesAsync(Guid channelId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var channel = await _channelRepository.GetByIdAsync(channelId);
            if (channel == null)
                throw new KeyNotFoundException("Channel not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == channel.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You do not have permission to view messages for this channel.");

            var messages = (await _messageRepository.FindAsync(m => m.ChannelId == channelId, "User"))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageResponse(
                    m.Id,
                    m.ChannelId,
                    m.UserId,
                    m.User?.FullName ?? string.Empty,
                    m.Content,
                    m.CreatedAt,
                    m.EditedAt,
                    m.UserId == userId.Value
                )).ToList();

            return messages;
        }

        public async Task<ChatMessageResponse> SendMessageAsync(Guid channelId, CreateChatMessageRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Content)) throw new ArgumentException("Message content is required.");

            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var channel = await _channelRepository.GetByIdAsync(channelId);
            if (channel == null)
                throw new KeyNotFoundException("Channel not found.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == channel.WorkspaceId, "Members");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You do not have permission to send messages to this channel.");

            var message = new ChatMessage
            {
                ChannelId = channelId,
                UserId = userId.Value,
                Content = request.Content.Trim()
            };

            await _messageRepository.AddAsync(message);
            await _messageRepository.SaveChangesAsync();

            var response = new ChatMessageResponse(
                message.Id,
                message.ChannelId,
                message.UserId,
                user.FullName ?? string.Empty,
                message.Content,
                message.CreatedAt,
                message.EditedAt,
                true
            );

            await _chatRealTimeService.NotifyMessageSentAsync(channelId, response);

            return response;
        }
    }
}
