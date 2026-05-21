using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartWorkspaceManager.Application.Interfaces;

namespace SmartWorkspaceManager.API.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return null;

                var userIdStr = httpContext.User.FindFirst("sub")?.Value 
                    ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdStr, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }

        public bool IsAuthenticated => 
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
