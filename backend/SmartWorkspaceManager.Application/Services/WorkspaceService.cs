using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.Common;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Application.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IGenericRepository<WorkspaceMember> _workspaceMemberRepository;
        private readonly IGenericRepository<WorkspaceInvitation> _workspaceInvitationRepository;
        private readonly IGenericRepository<Board> _boardRepository;
        private readonly IGenericRepository<Column> _columnRepository;
        private readonly IGenericRepository<BoardTask> _taskRepository;
        private readonly IGenericRepository<ChatChannel> _chatChannelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public WorkspaceService(
            IGenericRepository<Workspace> workspaceRepository,
            IGenericRepository<WorkspaceMember> workspaceMemberRepository,
            IGenericRepository<WorkspaceInvitation> workspaceInvitationRepository,
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Column> columnRepository,
            IGenericRepository<BoardTask> taskRepository,
            IGenericRepository<ChatChannel> chatChannelRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _workspaceMemberRepository = workspaceMemberRepository ?? throw new ArgumentNullException(nameof(workspaceMemberRepository));
            _workspaceInvitationRepository = workspaceInvitationRepository ?? throw new ArgumentNullException(nameof(workspaceInvitationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _columnRepository = columnRepository ?? throw new ArgumentNullException(nameof(columnRepository));
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _chatChannelRepository = chatChannelRepository ?? throw new ArgumentNullException(nameof(chatChannelRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<WorkspaceResponse> CreateWorkspaceAsync(CreateWorkspaceRequest request)
        {
            var ownerId = _userContext.UserId;
            if (ownerId == null || ownerId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User is not authenticated. Please provide a valid JWT token.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Workspace name is required.");
            }

            if (request.Name.Length > 200)
            {
                throw new ArgumentException("Workspace name cannot exceed 200 characters.");
            }

            if (request.Description?.Length > 1000)
            {
                throw new ArgumentException("Workspace description cannot exceed 1000 characters.");
            }

            var owner = await _userRepository.GetByIdAsync(ownerId.Value);
            if (owner == null)
            {
                throw new KeyNotFoundException("Owner user not found.");
            }

            // Create workspace entity
            var workspace = new Workspace
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                OwnerId = ownerId.Value
            };

            var member = new WorkspaceMember
            {
                UserId = ownerId.Value,
                WorkspaceId = workspace.Id,
                Role = WorkspaceRole.Owner,
                JoinedAt = DateTime.UtcNow
            };
            workspace.Members.Add(member);

            await _workspaceRepository.AddAsync(workspace);
            await _workspaceRepository.SaveChangesAsync();

            // Create default chat channel for the workspace
            var channel = new ChatChannel
            {
                Name = "General",
                Type = ChannelType.Workspace,
                WorkspaceId = workspace.Id
            };

            await _chatChannelRepository.AddAsync(channel);
            await _chatChannelRepository.SaveChangesAsync();

            return new WorkspaceResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                workspace.OwnerId,
                workspace.CreatedAt,
                workspace.UpdatedAt
            );
        }
        public async Task<WorkspaceResponse> InitializeWorkspaceAsync(CreateWorkspaceRequest request)
        {
            var ownerId = _userContext.UserId;
            if (ownerId == null || ownerId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User is not authenticated. Please provide a valid JWT token.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Workspace name is required.");
            }

            if (request.Name.Length > 200)
            {
                throw new ArgumentException("Workspace name cannot exceed 200 characters.");
            }

            var owner = await _userRepository.GetByIdAsync(ownerId.Value);
            if (owner == null)
            {
                throw new KeyNotFoundException("Owner user not found.");
            }

            var workspace = new Workspace
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                OwnerId = ownerId.Value
            };

            var member = new WorkspaceMember
            {
                UserId = ownerId.Value,
                WorkspaceId = workspace.Id,
                Role = WorkspaceRole.Owner,
                JoinedAt = DateTime.UtcNow
            };
            workspace.Members.Add(member);

            await _workspaceRepository.AddAsync(workspace);
            await _workspaceRepository.SaveChangesAsync();

            var board = new Board
            {
                WorkspaceId = workspace.Id,
                Name = "General",
                CreatedBy = ownerId.Value
            };

            await _boardRepository.AddAsync(board);
            await _boardRepository.SaveChangesAsync();

            var defaultColumns = new[]
            {
                new Column { BoardId = board.Id, Name = "Todo", Position = 1000 },
                new Column { BoardId = board.Id, Name = "In Progress", Position = 2000 },
                new Column { BoardId = board.Id, Name = "Review", Position = 3000 },
                new Column { BoardId = board.Id, Name = "Done", Position = 4000 }
            };

            foreach (var col in defaultColumns)
            {
                await _columnRepository.AddAsync(col);
            }

            await _columnRepository.SaveChangesAsync();

            // Create default chat channel for the workspace
            var defaultChannel = new ChatChannel
            {
                Name = "General",
                Type = ChannelType.Workspace,
                WorkspaceId = workspace.Id
            };

            await _chatChannelRepository.AddAsync(defaultChannel);
            await _chatChannelRepository.SaveChangesAsync();

            return new WorkspaceResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                workspace.OwnerId,
                workspace.CreatedAt,
                workspace.UpdatedAt
            );
        }
        public async Task<UserWorkspacesResponse> GetWorkspacesOfUserAsync()
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User is not authenticated. Please provide a valid JWT token.");
            }

            var workspaces = await _workspaceRepository.FindAsync(
                w => w.OwnerId == userId.Value || w.Members.Any(m => m.UserId == userId.Value),
                w => w.Members
            );

            var workspacesList = workspaces.ToList();

            return new UserWorkspacesResponse(
                TotalWorkspacesCount: workspacesList.Count,
                Workspaces: workspacesList.Select(w => new UserWorkspaceItemResponse(
                    w.Id,
                    w.Name,
                    w.Description,
                    w.OwnerId,
                    w.CreatedAt,
                    w.UpdatedAt,
                    MemberCount: w.Members.Count
                )).ToList()
            );
        }

        public async Task<WorkspaceDetailResponse> GetWorkspaceByIdAsync(Guid id, WorkspaceQueryOptions? options = null)
        {
            options ??= new WorkspaceQueryOptions();

            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated. Please provide a valid JWT token.");

            var workspaces = await _workspaceRepository.FindAsync(w => w.Id == id, "Members", "Owner");
            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMember = workspace.Members.Any(m => m.UserId == userId.Value) || workspace.OwnerId == userId.Value;
            if (!isMember)
                throw new UnauthorizedAccessException("You do not have permission to view this workspace.");

            var ownerName = workspace.Owner?.FullName ?? (await _userRepository.GetByIdAsync(workspace.OwnerId))?.FullName ?? string.Empty;

            var boardsAll = (await _boardRepository.FindAsync(b => b.WorkspaceId == workspace.Id, "Creator")).ToList();

            var boardsPaged = boardsAll
                .OrderBy(b => b.CreatedAt)
                .Skip((Math.Max(1, options.BoardsPage) - 1) * options.BoardsPageSize)
                .Take(options.BoardsPageSize)
                .ToList();

            var boardDtos = new List<BoardDto>();

            foreach (var board in boardsPaged)
            {
                var boardCreatorName = board.Creator?.FullName ?? (await _userRepository.GetByIdAsync(board.CreatedBy))?.FullName ?? string.Empty;

                var columns = (await _columnRepository.FindAsync(c => c.BoardId == board.Id, Array.Empty<string>()))
                    .OrderBy(c => c.Position)
                    .Select(c => new ColumnBasicDto(c.Id, c.BoardId, c.Name, c.Position))
                    .ToList();

                boardDtos.Add(new BoardDto(
                    board.Id,
                    board.WorkspaceId,
                    board.Name,
                    boardCreatorName,
                    board.CreatedAt,
                    board.UpdatedAt,
                    columns
                ));
            }

            var memberCount = workspace.Members?.Count ?? 0;

            return new WorkspaceDetailResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                ownerName,
                workspace.CreatedAt,
                workspace.UpdatedAt,
                memberCount,
                boardDtos
            );
        }

    }
}
