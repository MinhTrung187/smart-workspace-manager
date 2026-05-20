using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
