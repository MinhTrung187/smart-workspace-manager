using System;
using System.Threading.Tasks;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;
using SmartWorkspaceManager.Domain.Entities;

namespace SmartWorkspaceManager.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("Email is required.");
            }
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long.");
            }
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException("Full name is required.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var hashedPassword = _passwordHasher.Hash(request.Password);
            var user = new User
            {
                Email = normalizedEmail,
                FullName = request.FullName.Trim(),
                AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim(),
                PasswordHash = hashedPassword
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var tokenResult = _jwtTokenService.CreateToken(user);
            return new AuthResponse(
                tokenResult.Token,
                tokenResult.ExpiresAt,
                new UserResponse(user.Id, user.Email, user.FullName, user.AvatarUrl)
            );
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var tokenResult = _jwtTokenService.CreateToken(user);
            return new AuthResponse(
                tokenResult.Token,
                tokenResult.ExpiresAt,
                new UserResponse(user.Id, user.Email, user.FullName, user.AvatarUrl)
            );
        }
    }
}
