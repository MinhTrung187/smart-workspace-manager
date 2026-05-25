using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IWorkspaceInvitationService
    {
        Task<List<AllInvitationResponse>> GetInvitationsForCurrentUserAsync();
        Task<WorkspaceMemberDto> AcceptInvitationAsync(Guid invitationId);
    }
}
