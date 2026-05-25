using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Services
{
    public class WorkspaceInvitationService : IWorkspaceInvitationService
    {
        private readonly IGenericRepository<WorkspaceInvitation> _invitationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public WorkspaceInvitationService(
            IGenericRepository<WorkspaceInvitation> invitationRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
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
    }
}
