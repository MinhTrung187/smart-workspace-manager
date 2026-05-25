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

        public WorkspaceInvitationService(
            IGenericRepository<WorkspaceInvitation> invitationRepository,
            IUserRepository userRepository,
            IGenericRepository<WorkspaceMember> workspaceMemberRepository,
            IUserContext userContext)
        {
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _workspaceMemberRepository = workspaceMemberRepository ?? throw new ArgumentNullException(nameof(workspaceMemberRepository));
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
            if (invitation.Email.ToLower() != user.Email.ToLower())
            {
                throw new UnauthorizedAccessException("This invitation is not for the current user.");
            }
            if (invitation.Status != InvitationStatus.Pending)
            {
                throw new InvalidOperationException("This invitation has already been accepted or declined.");
            }
            if (invitation.ExpiredAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                _invitationRepository.Update(invitation);
                await _invitationRepository.SaveChangesAsync();

                throw new InvalidOperationException("This invitation has expired.");
            }
            var existingMembership = await _workspaceMemberRepository.FindAsync(
                wm => wm.WorkspaceId == invitation.WorkspaceId && wm.UserId == userId.Value,
                Array.Empty<string>()
            );
            if (existingMembership.Any())
            {
                invitation.Status = InvitationStatus.Accepted;
                _invitationRepository.Update(invitation);
                await _invitationRepository.SaveChangesAsync();
                throw new InvalidOperationException("You are already a member of this workspace.");
            }
            var member = new WorkspaceMember
            {
                UserId = userId.Value,
                WorkspaceId = invitation.WorkspaceId,
                Role = WorkspaceRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            invitation.Status = InvitationStatus.Accepted;
            _invitationRepository.Update(invitation);
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
    }

}
