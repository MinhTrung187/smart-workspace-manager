using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.Application.Services
{
    public class WorkspaceMemberService : IWorkspaceMemberService
    {
        private readonly IGenericRepository<WorkspaceMember> _workspaceMemberRepository;
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserContext _userContext;

        public WorkspaceMemberService(
            IGenericRepository<WorkspaceMember> workspaceMemberRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IActivityLogService activityLogService,
            IUserContext userContext)
        {
            _workspaceMemberRepository = workspaceMemberRepository ?? throw new ArgumentNullException(nameof(workspaceMemberRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        public async Task<List<WorkspaceMemberDto>> GetMembersByWorkspaceAsync(Guid workspaceId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            // Check permission: owner or member
            if (workspace.OwnerId != userId.Value)
            {
                var membershipCheck = await _workspaceMemberRepository.FindAsync(
                    wm => wm.WorkspaceId == workspaceId && wm.UserId == userId.Value,
                    Array.Empty<string>());

                if (!membershipCheck.Any())
                    throw new UnauthorizedAccessException("You do not have permission to view workspace members.");
            }

            // Load members including user navigation
            var members = await _workspaceMemberRepository.FindAsync(
                wm => wm.WorkspaceId == workspaceId,
                "User");

            return members
                .OrderBy(m => m.JoinedAt)
                .Select(m => new WorkspaceMemberDto(
                    m.UserId,
                    m.User?.FullName ?? string.Empty,
                    m.User?.Email ?? string.Empty,
                    m.User?.AvatarUrl,
                    m.Role.ToString(),
                    m.JoinedAt
                )).ToList();
        }

        public async Task RemoveMemberAsync(Guid workspaceId, Guid userId)
        {
            var currentUserId = _userContext.UserId;
            if (currentUserId == null || currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
            if (workspace == null)
                throw new KeyNotFoundException("Workspace not found.");

            if (workspace.OwnerId != currentUserId.Value)
                throw new UnauthorizedAccessException("You do not have permission to remove members from this workspace.");

            var memberToRemove = await _workspaceMemberRepository.FindAsync(
                wm => wm.WorkspaceId == workspaceId && wm.UserId == userId,
                Array.Empty<string>());

            if (!memberToRemove.Any())
                throw new KeyNotFoundException("Member not found in the workspace.");

            _workspaceMemberRepository.Delete(memberToRemove.First());
            var target = memberToRemove.First().User?.FullName ?? "Unknown User";
            await _activityLogService.LogAsync(
                ActivityType.MemberRemoved,
                workspaceId,
                description: $"Removed {target}");
             await _workspaceMemberRepository.SaveChangesAsync();
        }
    }
}
