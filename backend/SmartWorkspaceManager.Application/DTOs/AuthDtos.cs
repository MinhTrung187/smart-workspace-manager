using System;
using System.ComponentModel.DataAnnotations;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed class RegisterRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
    }

    public sealed class LoginRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public sealed record AuthResponse(
        string Token,
        DateTime ExpiresAt,
        UserResponse User);

    public sealed record UserResponse(
        Guid Id,
        string Email,
        string FullName,
        string? AvatarUrl);
}
