using System;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IUserContext
    {
        Guid? UserId { get; }
        bool IsAuthenticated { get; }
    }
}
