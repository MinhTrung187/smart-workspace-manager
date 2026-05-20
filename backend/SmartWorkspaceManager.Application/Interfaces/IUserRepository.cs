using System.Threading.Tasks;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}
