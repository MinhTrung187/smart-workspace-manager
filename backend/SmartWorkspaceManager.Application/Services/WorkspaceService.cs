using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Domain.Enums;

namespace SmartWorkspaceManager.Application.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IGenericRepository<Workspace> _workspaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;

        public WorkspaceService(
            IGenericRepository<Workspace> workspaceRepository,
            IUserRepository userRepository,
            IUserContext userContext)
        {
            _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

            return new WorkspaceResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                workspace.OwnerId,
                workspace.CreatedAt,
                workspace.UpdatedAt
            );
        }
    }
}
