using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Application.Services
{
    public class WorkspaceInvitationService : IWorkspaceInvitationService
    {
        private readonly IGenericRepository<WorkspaceInvitation> _invitationRepository;
        private readonly IGenericRepository<WorkspaceMember> _workspaceMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;
        private readonly IGenericRepository<Workspace> _workspaceRepository;

        public WorkspaceInvitationService(
            IGenericRepository<WorkspaceInvitation> invitationRepository,
            IUserRepository userRepository,
            IGenericRepository<WorkspaceMember> workspaceMemberRepository,
            IGenericRepository<Workspace> workspaceRepository,
            IUserContext userContext)
        {
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _workspaceMemberRepository = workspaceMemberRepository ?? throw new ArgumentNullException(nameof(workspaceMemberRepository));
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
        }

        public async Task<List<AllInvitationResponse>> GetInvitationsForCurrentUserAsync()
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User is not authenticated. Please provide a valid JWT token.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var invitations = await _invitationRepository.FindAsync(
                wi => wi.Email.ToLower() == user.Email.ToLower(),
                "Workspace"
            );

            return invitations
                .Select(wi => new AllInvitationResponse(
                    wi.Id,
                    wi.WorkspaceId,
                    wi.Workspace?.Name,
                    wi.Email,
                    wi.Status.ToString(),
                    wi.CreatedAt,
                    wi.ExpiredAt,
                    $"/invite/{wi.Id}"
                ))
                .ToList();
        }
        public async Task<WorkspaceMemberDto> AcceptInvitationAsync(Guid invitationId)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User is not authenticated. Please provide a valid JWT token.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var invitation = await _invitationRepository.GetByIdAsync(invitationId);
            if (invitation == null)
            {
                throw new KeyNotFoundException("Invitation not found.");
            }

            if (!string.Equals(invitation.Email?.Trim(), user.Email?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("This invitation is not for the current user.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                throw new InvalidOperationException("This invitation has already been accepted or declined.");
            }

            if (invitation.ExpiredAt <= DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                _invitationRepository.Update(invitation);
                await _invitationRepository.SaveChangesAsync();

                throw new InvalidOperationException("This invitation has expired.");
            }

            // Check existing membership
            var existingMembership = await _workspaceMemberRepository.FindAsync(
                wm => wm.WorkspaceId == invitation.WorkspaceId && wm.UserId == userId.Value,
                Array.Empty<string>()
            );

            if (existingMembership.Any())
            {
                // If already a member, mark invitation accepted and persist (no duplicate member)
                invitation.Status = InvitationStatus.Accepted;
                _invitationRepository.Update(invitation);
                await _invitationRepository.SaveChangesAsync();

                throw new InvalidOperationException("You are already a member of this workspace.");
            }

            // Create and persist membership, update invitation status, then save once.
            var member = new WorkspaceMember
            {
                UserId = userId.Value,
                WorkspaceId = invitation.WorkspaceId,
                Role = WorkspaceRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            await _workspaceMemberRepository.AddAsync(member);

            invitation.Status = InvitationStatus.Accepted;
            _invitationRepository.Update(invitation);

            // SaveChanges on either repository will persist both changes (shared DbContext)
            await _invitationRepository.SaveChangesAsync();

            return new WorkspaceMemberDto(
                member.UserId,
                user.FullName ?? string.Empty,
                user.Email,
                user.AvatarUrl,
                member.Role.ToString(),
                member.JoinedAt
            );
        }
        public async Task<WorkspaceInvitationResponse> CreateInvitationAsync(Guid workspaceId, InviteUserRequest request)
        {
            var userId = _userContext.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User is not authenticated. Please provide a valid JWT token.");
            }

            // 1. Validate workspace existence and owner permission
            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
            if (workspace == null)
            {
                throw new KeyNotFoundException("Workspace not found.");
            }

            if (workspace.OwnerId != userId.Value)
            {
                throw new UnauthorizedAccessException("Only the workspace owner can invite other users.");
            }

            var inviteEmail = request.Email.Trim().ToLower();

            // 2. Check if the user is already a member of the workspace
            var invitedUser = await _userRepository.GetByEmailAsync(inviteEmail);
            if (invitedUser != null)
            {
                var existingMembership = await _workspaceMemberRepository.FindAsync(
                    m => m.WorkspaceId == workspaceId && m.UserId == invitedUser.Id,
                    Array.Empty<string>()
                );

                if (existingMembership.Any())
                {
                    throw new InvalidOperationException("User is already a member of this workspace.");
                }
            }

            // 3. Check for an active pending invitation
            var existingInvitations = await _invitationRepository.FindAsync(
                i => i.WorkspaceId == workspaceId && i.Email.ToLower() == inviteEmail && i.Status == InvitationStatus.Pending,
                Array.Empty<string>()
            );

            var activeInvitation = existingInvitations.FirstOrDefault(i => i.ExpiredAt > DateTime.UtcNow);
            if (activeInvitation != null)
            {
                return new WorkspaceInvitationResponse(
                    activeInvitation.Id,
                    activeInvitation.WorkspaceId,
                    activeInvitation.Email,
                    activeInvitation.Status.ToString(),
                    activeInvitation.ExpiredAt,
                    $"/invite/{activeInvitation.Id}"
                );
            }

            // 4. Create the new invitation
            var invitation = new WorkspaceInvitation
            {
                WorkspaceId = workspaceId,
                Email = request.Email.Trim(),
                Status = InvitationStatus.Pending,
                ExpiredAt = DateTime.UtcNow.AddDays(7) // Invitation expires in 7 days
            };

            await _invitationRepository.AddAsync(invitation);
            await _invitationRepository.SaveChangesAsync();

            return new WorkspaceInvitationResponse(
                invitation.Id,
                invitation.WorkspaceId,
                invitation.Email,
                invitation.Status.ToString(),
                invitation.ExpiredAt,
                $"/invite/{invitation.Id}"
            );
        }
    }

}
