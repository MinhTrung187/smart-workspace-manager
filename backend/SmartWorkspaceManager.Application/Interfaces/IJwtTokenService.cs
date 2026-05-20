using System;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) CreateToken(User user);
    }
}
