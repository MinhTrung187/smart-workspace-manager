using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Services
{
    public class BoardService : IBoardService
    {
        private readonly IGenericRepository<Board> _boardRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public BoardService(
            IGenericRepository<Board> boardRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<BoardResponse> CreateBoardAsync(CreateBoardRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Board name is required.");

            if (request.Name.Length > 200)
                throw new ArgumentException("Board name cannot exceed 200 characters.");

            // Verify workspace exists and current user is member or owner
            var workspaces = await _workspaceRepository.FindAsync(
                w => w.Id == request.WorkspaceId,
                "Members"
            );

            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var board = new Board
            {
                WorkspaceId = request.WorkspaceId,
                Name = request.Name.Trim(),
                CreatedBy = userId.Value
            };

            await _boardRepository.AddAsync(board);
            await _boardRepository.SaveChangesAsync();

            return new BoardResponse(
                board.Id,
                board.WorkspaceId,
                board.Name,
                board.CreatedBy,
                board.CreatedAt,
                board.UpdatedAt
            );
        }

        public async Task<List<BoardResponse>> GetBoardsByWorkspaceAsync(Guid workspaceId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var workspaces = await _workspaceRepository.FindAsync(
                w => w.Id == workspaceId,
                "Members"
            );

            var workspace = workspaces.FirstOrDefault();
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of this workspace.");

            var boards = await _boardRepository.FindAsync(
                b => b.WorkspaceId == workspaceId,
                "Creator"
            );

            return boards.Select(b => new BoardResponse(
                b.Id,
                b.WorkspaceId,
                b.Name,
                b.CreatedBy,
                b.CreatedAt,
                b.UpdatedAt
            )).ToList();
        }

        public async Task<BoardResponse> GetBoardByIdAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var boards = await _boardRepository.FindAsync(
                b => b.Id == id,
                "Workspace", "Creator"
            );

            var board = boards.FirstOrDefault();
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspace = board.Workspace;
            if (workspace == null)
            {
                // load workspace to check membership
                var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
                workspace = workspaces.FirstOrDefault();
            }

            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isMemberOrOwner = workspace.OwnerId == userId.Value || workspace.Members.Any(m => m.UserId == userId.Value);
            if (!isMemberOrOwner)
                throw new UnauthorizedAccessException("You are not a member of the workspace for this board.");

            return new BoardResponse(
                board.Id,
                board.WorkspaceId,
                board.Name,
                board.CreatedBy,
                board.CreatedAt,
                board.UpdatedAt
            );
        }

        public async Task<BoardResponse> UpdateBoardAsync(Guid id, UpdateBoardRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Board name is required.");

            if (request.Name.Length > 200)
                throw new ArgumentException("Board name cannot exceed 200 characters.");

            var boards = await _boardRepository.FindAsync(
                b => b.Id == id,
                "Workspace"
            );

            var board = boards.FirstOrDefault();
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            // permission: allow workspace owner or board creator
            var workspace = board.Workspace;
            if (workspace == null)
            {
                var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
                workspace = workspaces.FirstOrDefault();
            }

            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == userId.Value || board.CreatedBy == userId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You don't have permission to update this board.");

            board.Name = request.Name.Trim();
            board.Touch();
            _boardRepository.Update(board);
            await _boardRepository.SaveChangesAsync();

            return new BoardResponse(
                board.Id,
                board.WorkspaceId,
                board.Name,
                board.CreatedBy,
                board.CreatedAt,
                board.UpdatedAt
            );
        }

        public async Task DeleteBoardAsync(Guid id)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var boards = await _boardRepository.FindAsync(
                b => b.Id == id,
                "Workspace"
            );

            var board = boards.FirstOrDefault();
            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            var workspace = board.Workspace;
            if (workspace == null)
            {
                var workspaces = await _workspaceRepository.FindAsync(w => w.Id == board.WorkspaceId, "Members");
                workspace = workspaces.FirstOrDefault();
            }

            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            var isAllowed = workspace.OwnerId == userId.Value || board.CreatedBy == userId.Value;
            if (!isAllowed)
                throw new UnauthorizedAccessException("You don't have permission to delete this board.");

            // soft-delete
            board.SoftDelete();
            _boardRepository.Update(board);
            await _boardRepository.SaveChangesAsync();
        }
    }
}
