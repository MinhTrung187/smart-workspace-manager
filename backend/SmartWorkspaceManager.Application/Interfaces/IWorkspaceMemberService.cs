using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IWorkspaceMemberService
    {
        Task<List<WorkspaceMemberDto>> GetMembersByWorkspaceAsync(Guid workspaceId);
        Task RemoveMemberAsync(Guid workspaceId, Guid userId);
    }
}
    