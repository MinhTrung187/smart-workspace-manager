using System;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceResponse> CreateWorkspaceAsync(CreateWorkspaceRequest request);
        Task<UserWorkspacesResponse> GetWorkspacesOfUserAsync();
        Task<WorkspaceDetailResponse> GetWorkspaceByIdAsync(Guid id);
        Task<WorkspaceResponse> InitializeWorkspaceAsync(CreateWorkspaceRequest request);

    }
}
