using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        public UserService(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl
            });
        }
    }
}
